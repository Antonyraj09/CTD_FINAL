using CTD_FINAL.Constants;
using CTD_FINAL.Data;
using CTD_FINAL.Data.Seed;
using CTD_FINAL.Entities;
using CTD_FINAL.Entities.Admin;
using CTD_FINAL.Enums;
using CTD_FINAL.Infrastructure;
using CTD_FINAL.Infrastructure.Identity;
using CTD_FINAL.Infrastructure.Middleware;
using CTD_FINAL.Infrastructure.Tenancy;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Repositories;
using CTD_FINAL.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/ctdsuite-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30));

builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("AppOptions"));

// Kept around only as the Development-only auto-provision target (see the seeding block
// near the bottom of this file) — AppDbContext itself no longer uses this directly. The
// app never has one fixed tenant connection string: every request resolves its own via
// ITenantContextAccessor (see below), keyed off the LicenseNumber the user signed in with.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddScoped<ITenantContextAccessor, TenantContextAccessor>();

// AppDbContext's connection is resolved per-request from whatever ITenantContextAccessor
// holds for the current scope, instead of one static connection string. AddDbContext's
// (IServiceProvider, DbContextOptionsBuilder) overload resolves services from the SAME
// scope the DbContext instance itself belongs to, so this correctly picks up a fresh
// tenant on every request — see TenantResolutionMiddleware / AccountController.Login for
// where ITenantContextAccessor actually gets populated.
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    // `dotnet ef` briefly builds this same host while probing for DbContext types, before
    // falling back to AppDbContextFactory (IDesignTimeDbContextFactory<AppDbContext>) — if
    // this lambda throws during that probe, the tool surfaces the exception instead of
    // trying the factory. EF.IsDesignTime is exactly the documented escape hatch: true only
    // under design-time tooling, never at real runtime.
    if (Microsoft.EntityFrameworkCore.EF.IsDesignTime)
    {
        options.UseSqlServer("Server=(local);Database=__DesignTimePlaceholder__;Trusted_Connection=True;");
        return;
    }

    var tenant = sp.GetRequiredService<ITenantContextAccessor>();
    if (string.IsNullOrEmpty(tenant.ConnectionString))
        throw new InvalidOperationException("No tenant database connection has been established for this request.");
    options.UseSqlServer(tenant.ConnectionString, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
});

// ADMIN_CTD: the one fixed, always-reachable database (Companies/Licenses/ClientDatabases/
// InstallationHistory) every tenant lookup starts from. Unlike AppDbContext above, this
// connection stays static for the life of the process.
var adminConnectionString = builder.Configuration.GetConnectionString("AdminConnection")
    ?? throw new InvalidOperationException("Connection string 'AdminConnection' not found.");

builder.Services.AddDbContext<AdminDbContext>(options =>
    options.UseSqlServer(adminConnectionString, sql => sql.MigrationsAssembly(typeof(AdminDbContext).Assembly.FullName)));

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddMemoryCache();

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IMasterCrudService<>), typeof(MasterCrudService<>));
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<INumberSequenceService, NumberSequenceService>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAlertSender, LoggedAlertSender>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IJobIsneService, JobIsneService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IPartyService, PartyService>();

builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<ITenantResolutionService, TenantResolutionService>();
builder.Services.AddScoped<ILicenseService, LicenseService>();
builder.Services.AddScoped<IProvisioningService, ProvisioningService>();

builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppUserClaimsPrincipalFactory>();

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("AppOptions:SessionTimeoutMinutes", 30));
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddAntiforgery(options =>
{
    // Lets AJAX POSTs with a JSON body (no form fields) supply the token via header instead.
    options.HeaderName = "RequestVerificationToken";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseGlobalExceptionMiddleware();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

app.UseStatusCodePagesWithReExecute("/Home/StatusCode", "?code={0}");
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseTenantResolution();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    // Only ADMIN_CTD gets migrated unconditionally at boot — it's the one database with a
    // fixed connection string. There is no longer a single "the" tenant database to seed
    // here: each tenant is provisioned (schema deployed + seeded) once, on demand, via the
    // Install Wizard / ProvisioningService.
    await AdminDbInitializer.SeedAsync(scope.ServiceProvider);

    // Development convenience only: if nothing has ever been installed, attach
    // DefaultConnection's database (already migrated via `dotnet ef database update
    // --context AppDbContext`) as the first tenant directly — no CREATE DATABASE/LOGIN/USER
    // needed since it already exists and the app connects to it the same way DefaultConnection
    // always has (Trusted_Connection), not a freshly-created SQL login. Seeds it (idempotent)
    // and registers it in ADMIN_CTD under the first generated license number (ERC00001, since
    // AdminNumberSequence starts empty). Production/Testing never take this path — they always
    // go through the real Install Wizard, which does create a brand new database/login.
    if (app.Environment.IsDevelopment())
    {
        var adminContext = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
        if (!await adminContext.Companies.AnyAsync())
        {
            const string devAdminEmail = "admin@ctdsuite.local";
            const string devAdminPassword = "ChangeMe#2026";

            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            await TenantSeeder.SeedNewTenantAsync(connectionString, loggerFactory, devAdminEmail, "System Administrator", devAdminPassword);

            var company = new Company
            {
                CompanyName = "Himalayan Cargo Movers Pvt. Ltd.",
                CompanyCode = "DEFAULT",
                Country = "India",
                Email = devAdminEmail,
                InstallationLocation = "Local Development",
                Status = CompanyStatus.Active
            };
            adminContext.Companies.Add(company);
            await adminContext.SaveChangesAsync();

            var licenseService = scope.ServiceProvider.GetRequiredService<ILicenseService>();
            var license = await licenseService.GenerateLicenseAsync(company.Id, company.CompanyCode, LicenseType.Trial, LicenseConstants.CurrentApplicationVersion);
            adminContext.Licenses.Add(license);

            var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
            var devConnectionBuilder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            adminContext.ClientDatabases.Add(new ClientDatabase
            {
                CompanyId = company.Id,
                DatabaseName = devConnectionBuilder.InitialCatalog,
                ServerName = devConnectionBuilder.DataSource,
                DatabaseUsername = "(Trusted_Connection)",
                EncryptedPassword = encryptionService.Encrypt(string.Empty),
                EncryptedConnectionString = encryptionService.Encrypt(connectionString),
                DatabaseVersion = LicenseConstants.CurrentApplicationVersion,
                ApplicationVersion = LicenseConstants.CurrentApplicationVersion,
                Status = ClientDatabaseStatus.Active
            });
            await adminContext.SaveChangesAsync();

            Log.Information("Development bootstrap registered {DatabaseName} as the first tenant under license {LicenseNumber} — sign in with {Email} / {Password}.",
                devConnectionBuilder.InitialCatalog, license.LicenseNumber, devAdminEmail, devAdminPassword);
        }
    }
}

app.Run();
