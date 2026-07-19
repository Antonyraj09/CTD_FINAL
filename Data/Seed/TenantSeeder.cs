using CTD_FINAL.Entities;
using CTD_FINAL.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CTD_FINAL.Data.Seed;

/// <summary>
/// Seeds a freshly-provisioned tenant database with exactly what the app needs to
/// function — Roles, the default Permission Matrix, one Administrator account, and
/// one default AppSettings row — and nothing else. Invoked once per new tenant, by
/// ProvisioningService, against that tenant's own AppDbContext (never at general app
/// boot, and never re-run against an already-seeded tenant).
///
/// This replaces the old DbInitializer, which additionally seeded four demo users and
/// a full set of sample master data (importers/agents/transporters/commodities/routes/
/// border points/customs houses/alert rules) — that's exactly the "business data" the
/// installation spec explicitly says must NOT be inserted, so none of it carries over
/// here. A brand-new tenant starts with a genuinely empty master-data/job history and
/// one real login to get started with.
/// </summary>
public static class TenantSeeder
{
    /// <summary>Builds an isolated mini DI container against the given connection string and
    /// seeds it — for callers that don't already have a scope pointed at the target tenant
    /// database (ProvisioningService mid-install, or Program.cs's Development bootstrap
    /// attaching an already-migrated database directly).</summary>
    public static async Task SeedNewTenantAsync(string connectionString, ILoggerFactory loggerFactory, string adminEmail, string adminFullName, string adminPassword)
    {
        var services = new ServiceCollection();
        services.AddSingleton(loggerFactory);
        services.AddLogging();
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        await SeedNewTenantAsync(scope.ServiceProvider, adminEmail, adminFullName, adminPassword);
    }

    public static async Task SeedNewTenantAsync(IServiceProvider services, string adminEmail, string adminFullName, string adminPassword)
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("TenantSeeder");

        await SeedRolesAsync(services);
        await SeedDefaultAdminAsync(services, adminEmail, adminFullName, adminPassword);
        await SeedPermissionsAsync(context);
        await SeedSettingsAsync(context);

        logger.LogInformation("Tenant seed complete for {Email}", adminEmail);
    }

    private static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        foreach (var role in RoleNames.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new ApplicationRole(role));
        }
    }

    private static async Task SeedDefaultAdminAsync(IServiceProvider services, string email, string fullName, string password)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        if (await userManager.FindByEmailAsync(email) is not null) return;

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true,
            IsActive = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException("Failed to create the default administrator account: " + string.Join("; ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(user, RoleNames.Administrator);
    }

    private static async Task SeedPermissionsAsync(AppDbContext context)
    {
        if (await context.RolePermissions.AnyAsync()) return;

        // Same default permission matrix as the original single-tenant seed —
        // Administrator=Customs Admin, Manager=Operations User, Operator=Accounts User, Viewer=Customer.
        var matrix = new (string ModuleKey, bool Admin, bool Manager, bool Operator, bool Viewer)[]
        {
            (PermissionKeys.DashboardView, true, true, true, false),
            (PermissionKeys.CustomerDashboardView, true, true, true, true),
            (PermissionKeys.JobCreateEdit, true, true, false, false),
            (PermissionKeys.JobClose, true, false, false, false),
            (PermissionKeys.TrackingView, true, true, true, false),
            (PermissionKeys.DocumentUpload, true, true, false, false),
            (PermissionKeys.MasterDataManage, true, false, false, false),
            (PermissionKeys.InvoiceGenerate, true, false, true, false),
            (PermissionKeys.ReportsViewExport, true, true, true, false),
            (PermissionKeys.UsersRolesManage, true, false, false, false),
            (PermissionKeys.AuditHistoryView, true, true, false, false),
            (PermissionKeys.AlertRulesManage, true, false, false, false),
            (PermissionKeys.JobIsneManage, true, true, false, false),
        };

        foreach (var row in matrix)
        {
            context.RolePermissions.AddRange(
                new RolePermission { Role = RoleNames.Administrator, ModuleKey = row.ModuleKey, Allowed = row.Admin },
                new RolePermission { Role = RoleNames.Manager, ModuleKey = row.ModuleKey, Allowed = row.Manager },
                new RolePermission { Role = RoleNames.Operator, ModuleKey = row.ModuleKey, Allowed = row.Operator },
                new RolePermission { Role = RoleNames.Viewer, ModuleKey = row.ModuleKey, Allowed = row.Viewer });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedSettingsAsync(AppDbContext context)
    {
        if (await context.AppSettings.AnyAsync()) return;
        context.AppSettings.Add(new AppSettingsEntity());
        await context.SaveChangesAsync();
    }
}
