using System.Text.RegularExpressions;
using CTD_FINAL.Constants;
using CTD_FINAL.Data;
using CTD_FINAL.Data.Seed;
using CTD_FINAL.Entities.Admin;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

/// <summary>
/// Installation Wizard Step 3 (create the client SQL Server database/login/user, assign
/// db_owner, deploy the full application schema, seed roles/permissions/one Administrator
/// account) and Step 4 (register Company/License/ClientDatabase rows in ADMIN_CTD).
/// </summary>
public class ProvisioningService : IProvisioningService
{
    // Every identifier is also passed through QUOTENAME() server-side before use in dynamic
    // DDL, but rejecting anything unexpected this early keeps failures clear and avoids
    // relying on QUOTENAME as the only line of defense against identifier injection.
    private static readonly Regex IdentifierPattern = new(@"^[A-Za-z][A-Za-z0-9_]{2,62}$", RegexOptions.Compiled);

    // Mirrors the Identity password policy TenantSeeder's UserManager actually enforces
    // (RequireUppercase/RequireDigit/RequireNonAlphanumeric + Identity's RequireLowercase
    // default) — InstallProvisionRequest's own [RegularExpression] already checks this at
    // model-binding time, but a request built directly against this service (bypassing MVC
    // model validation) still needs the same guard, and it belongs here before any SQL work
    // starts rather than failing deep inside TenantSeeder after the database/schema already
    // exist.
    private static readonly Regex AdminPasswordPolicy = new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$", RegexOptions.Compiled);

    private readonly AdminDbContext _adminContext;
    private readonly IEncryptionService _encryptionService;
    private readonly ILicenseService _licenseService;
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ProvisioningService> _logger;

    public ProvisioningService(
        AdminDbContext adminContext,
        IEncryptionService encryptionService,
        ILicenseService licenseService,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        ILogger<ProvisioningService> logger)
    {
        _adminContext = adminContext;
        _encryptionService = encryptionService;
        _licenseService = licenseService;
        _configuration = configuration;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<ProvisioningResult> ProvisionAsync(ProvisioningRequest request, CancellationToken ct = default)
    {
        if (!IdentifierPattern.IsMatch(request.DatabaseName))
            return new ProvisioningResult(false, null, null, "Database name must start with a letter and contain only letters, digits and underscores (3-63 characters).");
        if (!IdentifierPattern.IsMatch(request.DatabaseUsername))
            return new ProvisioningResult(false, null, null, "Database username must start with a letter and contain only letters, digits and underscores (3-63 characters).");
        if (!AdminPasswordPolicy.IsMatch(request.AdminPassword))
            return new ProvisioningResult(false, null, null, "Administrator password must be at least 8 characters and include an uppercase letter, a lowercase letter, a digit, and a symbol.");

        // A Company row has no uniqueness constraint of its own — without this check, retrying
        // the same client's details after a partial failure (Company created, then something
        // later failed) would silently create a second Company for the same code instead of
        // erroring, since ProvisionAsync always inserts a brand-new row. Resume (from the
        // pending-installation screen) is the intended path for finishing an existing attempt;
        // a genuinely new client needs its own, different Company Code.
        var existingCompany = await _adminContext.Companies.FirstOrDefaultAsync(c => c.CompanyCode == request.CompanyCode, ct);
        if (existingCompany is not null)
            return new ProvisioningResult(false, null, null, $"A client with company code '{request.CompanyCode}' already exists (\"{existingCompany.CompanyName}\"). If an earlier installation for it didn't finish, resume it from the pending-installation screen instead, or choose a different Company Code for a genuinely new client.");

        var provisioningConnectionString = _configuration.GetConnectionString("ProvisioningConnection")
            ?? throw new InvalidOperationException("Connection string 'ProvisioningConnection' not found.");

        // Snapshot the request up front (passwords excluded) so a failure that happens before
        // a Company row exists still leaves enough for the Installed Clients screen to show
        // what was attempted and let the operator resume without retyping everything.
        var history = new InstallationHistory
        {
            InstallationDate = DateTime.UtcNow,
            InstalledBy = request.InstalledBy,
            MachineName = request.MachineName,
            ApplicationVersion = LicenseConstants.CurrentApplicationVersion,
            InstallationStatus = InstallationStatus.Started,
            CompanyName = request.CompanyName,
            CompanyCode = request.CompanyCode,
            Address = request.Address,
            Country = request.Country,
            State = request.State,
            City = request.City,
            GstNumber = request.GstNumber,
            ContactPerson = request.ContactPerson,
            Email = request.Email,
            Phone = request.Phone,
            InstallationLocation = request.InstallationLocation,
            LicenseType = request.LicenseType.ToString(),
            DatabaseName = request.DatabaseName,
            DatabaseUsername = request.DatabaseUsername,
            AdminFullName = request.AdminFullName,
            AdminEmail = request.AdminEmail
        };
        _adminContext.InstallationHistories.Add(history);
        await _adminContext.SaveChangesAsync(ct);

        try
        {
            var serverBuilder = new SqlConnectionStringBuilder(provisioningConnectionString);
            var serverName = serverBuilder.DataSource;

            await CreateDatabaseAsync(serverBuilder.ConnectionString, request.DatabaseName, ct);
            await CreateLoginAsync(serverBuilder.ConnectionString, request.DatabaseUsername, request.DatabasePassword, ct);

            // MultipleActiveResultSets must be off here even if ProvisioningConnection has it on
            // — this connection only ever runs one thing at a time (CreateUserAndAssignRoleAsync,
            // then EF's migrator below), so it never needs concurrent result sets.
            var tenantAdminBuilder = new SqlConnectionStringBuilder(provisioningConnectionString) { InitialCatalog = request.DatabaseName, MultipleActiveResultSets = false };

            await using (var tenantConnection = new SqlConnection(tenantAdminBuilder.ConnectionString))
            {
                await tenantConnection.OpenAsync(ct);
                await CreateUserAndAssignRoleAsync(tenantConnection, request.DatabaseUsername, ct);
            }

            // EF Core's own migrator, not the static database/scripts/01_InitialCreate.sql script:
            // MigrateAsync only ever generates and sends SQL for migrations not yet recorded in
            // __EFMigrationsHistory, so re-running it against a database that's already partway
            // (or fully) migrated — the Resume path's whole point — is safe by construction. The
            // static idempotent script can't offer that guarantee for every migration: one that
            // adds a temporary column, uses it, then drops it within the same migration (e.g.
            // MergePartyMaster's LegacyImporterId) generates a batch that still *references* that
            // column even though its IF-guard would skip it at runtime — and SQL Server resolves
            // column names against an existing table at compile time regardless of which branch
            // of an IF actually runs, so the batch fails with "Invalid column name" once that
            // column is gone, on a second run against an already-migrated database. A real
            // install hit exactly this resuming after an unrelated first-attempt failure.
            var migrationOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(tenantAdminBuilder.ConnectionString, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                .Options;
            await using (var migrationContext = new AppDbContext(migrationOptions))
            {
                await migrationContext.Database.MigrateAsync(ct);
            }

            await SeedTenantAsync(tenantAdminBuilder.ConnectionString, request);

            // The connection stored for ongoing runtime use authenticates as the SQL login
            // just created above, not the elevated provisioning identity used to build the
            // database — this is the connection TenantResolutionService decrypts and hands
            // to AppDbContext on every subsequent request for this tenant.
            var runtimeConnectionBuilder = new SqlConnectionStringBuilder
            {
                DataSource = serverName,
                InitialCatalog = request.DatabaseName,
                UserID = request.DatabaseUsername,
                Password = request.DatabasePassword,
                TrustServerCertificate = serverBuilder.TrustServerCertificate,
                MultipleActiveResultSets = true
            };

            var company = new Company
            {
                CompanyName = request.CompanyName,
                CompanyCode = request.CompanyCode,
                Address = request.Address,
                Country = request.Country,
                State = request.State,
                City = request.City,
                GstNumber = request.GstNumber,
                ContactPerson = request.ContactPerson,
                Email = request.Email,
                Phone = request.Phone,
                InstallationLocation = request.InstallationLocation,
                Status = CompanyStatus.Active
            };
            _adminContext.Companies.Add(company);
            await _adminContext.SaveChangesAsync(ct);

            // Linked immediately, not after License/ClientDatabase creation below: if either of
            // those steps throws, the Company row genuinely exists in ADMIN_CTD by this point,
            // and the history row needs to say so — not look like a fully orphaned attempt with
            // nothing to show for it (which is exactly what happened before this fix: a real
            // Company got created, a later step failed, and the operator had no way to tell the
            // two apart from "Installed Clients").
            history.CompanyId = company.Id;
            await _adminContext.SaveChangesAsync(ct);

            var license = await _licenseService.GenerateLicenseAsync(company.Id, company.CompanyCode, request.LicenseType, LicenseConstants.CurrentApplicationVersion, ct);
            _adminContext.Licenses.Add(license);

            var clientDatabase = new ClientDatabase
            {
                CompanyId = company.Id,
                DatabaseName = request.DatabaseName,
                ServerName = serverName ?? string.Empty,
                DatabaseUsername = request.DatabaseUsername,
                EncryptedPassword = _encryptionService.Encrypt(request.DatabasePassword),
                EncryptedConnectionString = _encryptionService.Encrypt(runtimeConnectionBuilder.ConnectionString),
                DatabaseVersion = LicenseConstants.CurrentApplicationVersion,
                ApplicationVersion = LicenseConstants.CurrentApplicationVersion,
                Status = ClientDatabaseStatus.Active
            };
            _adminContext.ClientDatabases.Add(clientDatabase);

            history.InstallationStatus = InstallationStatus.Succeeded;

            await _adminContext.SaveChangesAsync(ct);

            _logger.LogInformation("Provisioned tenant database {DatabaseName} for company {CompanyCode} under license {LicenseNumber}.", request.DatabaseName, company.CompanyCode, license.LicenseNumber);
            return new ProvisioningResult(true, license.LicenseNumber, company.CompanyCode, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Provisioning failed for company {CompanyCode} / database {DatabaseName}.", request.CompanyCode, request.DatabaseName);
            history.InstallationStatus = InstallationStatus.Failed;
            var errorText = ex.ToString();
            history.ErrorLog = errorText.Length > 4000 ? errorText[..4000] : errorText;
            await _adminContext.SaveChangesAsync(ct);
            return new ProvisioningResult(false, null, null, ex.Message);
        }
    }

    private static async Task CreateDatabaseAsync(string serverConnectionString, string databaseName, CancellationToken ct)
    {
        await using var connection = new SqlConnection(serverConnectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        // CREATE DATABASE can't run inside a user transaction and can't take the database
        // name as an ordinary query parameter — QUOTENAME() applies proper identifier
        // quoting server-side to a parameterized value instead of string-concatenating it.
        command.CommandText = "IF DB_ID(@dbName) IS NULL BEGIN DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@dbName) + N';'; EXEC sp_executesql @sql; END";
        command.Parameters.AddWithValue("@dbName", databaseName);
        command.CommandTimeout = 120;
        await command.ExecuteNonQueryAsync(ct);
    }

    private static async Task CreateLoginAsync(string serverConnectionString, string loginName, string password, CancellationToken ct)
    {
        await using var connection = new SqlConnection(serverConnectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        // QUOTENAME(@password, '''') escapes the password for safe use as a quoted string
        // literal (doubling embedded single quotes) — the same server-side quoting approach
        // used for identifiers above, applied to a value instead of a name. CHECK_POLICY is
        // off because password strength is enforced by the Install Wizard's own validation,
        // not SQL Server's local policy (which may not even be configured on the target box).
        command.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = @loginName)
BEGIN
    DECLARE @sql nvarchar(max) = N'CREATE LOGIN ' + QUOTENAME(@loginName) + N' WITH PASSWORD = ' + QUOTENAME(@password, '''') + N', CHECK_POLICY = OFF;';
    EXEC sp_executesql @sql;
END";
        command.Parameters.AddWithValue("@loginName", loginName);
        command.Parameters.AddWithValue("@password", password);
        await command.ExecuteNonQueryAsync(ct);
    }

    private static async Task CreateUserAndAssignRoleAsync(SqlConnection tenantConnection, string userName, CancellationToken ct)
    {
        await using var command = tenantConnection.CreateCommand();
        // sp_addrolemember instead of ALTER ROLE ... ADD MEMBER: the latter only parses on SQL
        // Server 2012+ ("Incorrect syntax near the keyword 'ADD'" on anything older, e.g. 2008
        // R2 Express) — sp_addrolemember does the same db_owner grant and has worked unchanged
        // since SQL 2000, keeping this in line with the rest of the app's 2008 SP1+ floor. It
        // also needs no QUOTENAME/dynamic SQL of its own: @userName is passed straight through
        // as a stored-procedure parameter, not spliced into DDL text.
        command.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @userName)
BEGIN
    DECLARE @sql nvarchar(max) = N'CREATE USER ' + QUOTENAME(@userName) + N' FOR LOGIN ' + QUOTENAME(@userName) + N';';
    EXEC sp_executesql @sql;
END
EXEC sp_addrolemember N'db_owner', @userName;";
        command.Parameters.AddWithValue("@userName", userName);
        await command.ExecuteNonQueryAsync(ct);
    }

    // Runs against an isolated mini DI container, scoped to just this one seed call — the
    // main app's AppDbContext registration resolves its connection from ITenantContextAccessor
    // (a per-HTTP-request value), which doesn't exist here since provisioning isn't a login.
    private Task SeedTenantAsync(string tenantConnectionString, ProvisioningRequest request) =>
        TenantSeeder.SeedNewTenantAsync(tenantConnectionString, _loggerFactory, request.AdminEmail, request.AdminFullName, request.AdminPassword);
}
