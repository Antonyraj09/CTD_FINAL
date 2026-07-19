using System.Text.RegularExpressions;
using CTD_FINAL.Constants;
using CTD_FINAL.Data;
using CTD_FINAL.Data.Seed;
using CTD_FINAL.Entities.Admin;
using CTD_FINAL.Enums;
using CTD_FINAL.Infrastructure.Provisioning;
using CTD_FINAL.Interfaces;
using Microsoft.Data.SqlClient;

namespace CTD_FINAL.Services;

/// <summary>
/// Installation Wizard Step 3 (create the client SQL Server database/login/user, assign
/// db_owner, deploy the full application schema, seed roles/permissions/one Administrator
/// account) and Step 4 (register Company/License/ClientDatabase rows in ADMIN_CTD).
///
/// ALTER ROLE ... ADD MEMBER (SQL Server 2012+) is used for the db_owner grant instead of
/// the SQL-2008-compatible sp_addrolemember — a deliberate, narrower floor than the rest of
/// this app's general SQL Server 2008 SP1+ target, scoped to this feature only.
/// </summary>
public class ProvisioningService : IProvisioningService
{
    // Every identifier is also passed through QUOTENAME() server-side before use in dynamic
    // DDL, but rejecting anything unexpected this early keeps failures clear and avoids
    // relying on QUOTENAME as the only line of defense against identifier injection.
    private static readonly Regex IdentifierPattern = new(@"^[A-Za-z][A-Za-z0-9_]{2,62}$", RegexOptions.Compiled);

    private readonly AdminDbContext _adminContext;
    private readonly IEncryptionService _encryptionService;
    private readonly ILicenseService _licenseService;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ProvisioningService> _logger;

    public ProvisioningService(
        AdminDbContext adminContext,
        IEncryptionService encryptionService,
        ILicenseService licenseService,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILoggerFactory loggerFactory,
        ILogger<ProvisioningService> logger)
    {
        _adminContext = adminContext;
        _encryptionService = encryptionService;
        _licenseService = licenseService;
        _configuration = configuration;
        _environment = environment;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public async Task<ProvisioningResult> ProvisionAsync(ProvisioningRequest request, CancellationToken ct = default)
    {
        if (!IdentifierPattern.IsMatch(request.DatabaseName))
            return new ProvisioningResult(false, null, null, "Database name must start with a letter and contain only letters, digits and underscores (3-63 characters).");
        if (!IdentifierPattern.IsMatch(request.DatabaseUsername))
            return new ProvisioningResult(false, null, null, "Database username must start with a letter and contain only letters, digits and underscores (3-63 characters).");

        var provisioningConnectionString = _configuration.GetConnectionString("ProvisioningConnection")
            ?? throw new InvalidOperationException("Connection string 'ProvisioningConnection' not found.");

        var history = new InstallationHistory
        {
            InstallationDate = DateTime.UtcNow,
            InstalledBy = request.InstalledBy,
            MachineName = request.MachineName,
            ApplicationVersion = LicenseConstants.CurrentApplicationVersion,
            InstallationStatus = InstallationStatus.Started
        };
        _adminContext.InstallationHistories.Add(history);
        await _adminContext.SaveChangesAsync(ct);

        try
        {
            var serverBuilder = new SqlConnectionStringBuilder(provisioningConnectionString);
            var serverName = serverBuilder.DataSource;

            await CreateDatabaseAsync(serverBuilder.ConnectionString, request.DatabaseName, ct);
            await CreateLoginAsync(serverBuilder.ConnectionString, request.DatabaseUsername, request.DatabasePassword, ct);

            var tenantAdminBuilder = new SqlConnectionStringBuilder(provisioningConnectionString) { InitialCatalog = request.DatabaseName };

            await using (var tenantConnection = new SqlConnection(tenantAdminBuilder.ConnectionString))
            {
                await tenantConnection.OpenAsync(ct);
                await CreateUserAndAssignRoleAsync(tenantConnection, request.DatabaseUsername, ct);

                var scriptPath = Path.Combine(_environment.ContentRootPath, "database", "scripts", "01_InitialCreate.sql");
                var script = await File.ReadAllTextAsync(scriptPath, ct);
                await SqlScriptRunner.ExecuteAsync(tenantConnection, script, ct);
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

            history.CompanyId = company.Id;
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
        command.CommandText = @"
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @userName)
BEGIN
    DECLARE @sql nvarchar(max) = N'CREATE USER ' + QUOTENAME(@userName) + N' FOR LOGIN ' + QUOTENAME(@userName) + N';';
    EXEC sp_executesql @sql;
END
DECLARE @roleSql nvarchar(max) = N'ALTER ROLE db_owner ADD MEMBER ' + QUOTENAME(@userName) + N';';
EXEC sp_executesql @roleSql;";
        command.Parameters.AddWithValue("@userName", userName);
        await command.ExecuteNonQueryAsync(ct);
    }

    // Runs against an isolated mini DI container, scoped to just this one seed call — the
    // main app's AppDbContext registration resolves its connection from ITenantContextAccessor
    // (a per-HTTP-request value), which doesn't exist here since provisioning isn't a login.
    private Task SeedTenantAsync(string tenantConnectionString, ProvisioningRequest request) =>
        TenantSeeder.SeedNewTenantAsync(tenantConnectionString, _loggerFactory, request.AdminEmail, request.AdminFullName, request.AdminPassword);
}
