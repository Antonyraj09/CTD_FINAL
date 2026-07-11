using CTD_FINAL.Entities;
using CTD_FINAL.Constants;
using CTD_FINAL.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CTD_FINAL.Data.Seed;

/// <summary>
/// Reproduces the prototype's seed data: 5 masters (unified Party plus
/// Commodity/Route/BorderPoint/CustomsHouse), default users (DB.users, one
/// per role), the permission matrix
/// (PERMISSION_MATRIX_DEFAULT, renamed to the spec's role names), default
/// alert rules, numbering settings and company profile.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<AppDbContext>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        await context.Database.MigrateAsync();

        await SeedRolesAsync(services);
        await SeedUsersAsync(services);
        await SeedPermissionsAsync(context);
        await SeedMastersAsync(context);
        await SeedSettingsAsync(context);
        await SeedAlertRulesAsync(context);

        logger.LogInformation("Database seed complete.");
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

    private static async Task SeedUsersAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        var seedUsers = new (string Email, string FullName, string Role)[]
        {
            ("admin@ctdsuite.com", "Anil Sharma", RoleNames.Administrator),
            ("manager@ctdsuite.com", "Priya Nair", RoleNames.Manager),
            ("operator@ctdsuite.com", "Rajesh Gupta", RoleNames.Operator),
            ("viewer@ctdsuite.com", "Bina Thapa", RoleNames.Viewer),
        };

        foreach (var (email, fullName, role) in seedUsers)
        {
            if (await userManager.FindByEmailAsync(email) is not null) continue;

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await userManager.CreateAsync(user, "ChangeMe#2026");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(user, role);
        }
    }

    private static async Task SeedPermissionsAsync(AppDbContext context)
    {
        if (await context.RolePermissions.AnyAsync()) return;

        // Prototype's PERMISSION_MATRIX_DEFAULT, mapped:
        // Administrator=Customs Admin, Manager=Operations User, Operator=Accounts User, Viewer=Customer
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

    private static async Task SeedMastersAsync(AppDbContext context)
    {
        if (!await context.Parties.AnyAsync())
        {
            context.Parties.AddRange(
                // Importers
                new Party { Name = "Himalaya Steel Traders Pvt Ltd", IsImporter = true, Gstin = "10AABCH5566D1Z3", City = "Patna", Phone = "+91 98230 11234", Email = "ops@himalayasteel.com" },
                new Party { Name = "Annapurna Foods Imports", IsImporter = true, Gstin = "22AAACA1234B1Z9", City = "Raxaul", Phone = "+91 99110 44521", Email = "contact@annapurnafoods.in" },
                new Party { Name = "Everest Polymers Pvt Ltd", IsImporter = true, Gstin = "07AAFCE7788K1Z2", City = "Kolkata", Phone = "+91 98301 22390", Email = "logistics@everestpolymers.com" },
                new Party { Name = "Kathmandu Auto Parts Co.", IsImporter = true, Gstin = "19AAGCK9090M1Z1", City = "Kolkata", Phone = "+91 98740 55210", Email = "info@kathmanduauto.com.np" },
                new Party { Name = "Gorkha Cement Industries", IsImporter = true, Gstin = "10AAJCG4455L1Z6", City = "Patna", Phone = "+91 94310 88761", Email = "procurement@gorkhacement.com" },
                new Party { Name = "Lumbini Pharma Distributors", IsImporter = true, Gstin = "06AAKCL2233N1Z4", City = "Gurugram", Phone = "+91 98100 67321", Email = "supply@lumbinipharma.in" },

                // Agents (CHA)
                new Party { Name = "Patel CHA Services", IsAgent = true, License = "CHA/KOL/0091", City = "Kolkata", Phone = "+91 98300 12233", Email = "patel.cha@logimail.com" },
                new Party { Name = "Eastern Border Clearing Agency", IsAgent = true, License = "CHA/RXL/0042", City = "Raxaul", Phone = "+91 94300 99887", Email = "ebca@logimail.com" },
                new Party { Name = "Siliguri Transit Consultants", IsAgent = true, License = "CHA/SIL/0117", City = "Siliguri", Phone = "+91 98765 44120", Email = "siliguri.tc@logimail.com" },
                // Also runs its own fleet across the border — demonstrates a party holding more than one role.
                new Party { Name = "Birgunj Forwarding Associates", IsAgent = true, IsTransporter = true, License = "CHA/BGJ/0205", Fleet = "12 Trailers", City = "Birgunj (Nepal liaison)", Phone = "+977 98410 23456", Email = "bfa@logimail.com" },

                // Transporters
                new Party { Name = "Himgiri Road Carriers", IsTransporter = true, Fleet = "42 Trailers", City = "Kolkata", Phone = "+91 98300 55678", Email = "fleet@himgiritransport.com" },
                new Party { Name = "National Highway Logistics Pvt Ltd", IsTransporter = true, Fleet = "68 Trailers", City = "Patna", Phone = "+91 99551 23410", Email = "ops@nhlogistics.in" },
                new Party { Name = "Saptkoshi Carriers", IsTransporter = true, Fleet = "25 Trailers", City = "Raxaul", Phone = "+91 94300 77654", Email = "contact@saptkoshicarriers.com" },
                new Party { Name = "Konkan Rail-Road Movers", IsTransporter = true, Fleet = "Rail Rake + 30 Trailers", City = "Kolkata", Phone = "+91 98301 99001", Email = "krrm@logimail.com" });
        }

        if (!await context.Commodities.AnyAsync())
        {
            context.Commodities.AddRange(
                new Commodity { Name = "Iron & Steel Coils", HsCode = "7208.10" },
                new Commodity { Name = "Packaged Food Items", HsCode = "1901.90" },
                new Commodity { Name = "Plastic Granules (Polymer)", HsCode = "3901.20" },
                new Commodity { Name = "Automobile Spare Parts", HsCode = "8708.99" },
                new Commodity { Name = "Cement (Bulk/Bagged)", HsCode = "2523.29" },
                new Commodity { Name = "Pharmaceutical Products", HsCode = "3004.90" },
                new Commodity { Name = "Electrical Machinery", HsCode = "8517.12" },
                new Commodity { Name = "Textile & Garments", HsCode = "6204.43" });
        }

        if (!await context.TransitRoutes.AnyAsync())
        {
            context.TransitRoutes.AddRange(
                new TransitRoute { Name = "Kolkata Port → Raxaul → Birgunj", Distance = "720 km" },
                new TransitRoute { Name = "Kolkata Port → Jogbani → Biratnagar", Distance = "680 km" },
                new TransitRoute { Name = "Haldia Port → Panitanki → Kakarbhitta", Distance = "790 km" },
                new TransitRoute { Name = "Visakhapatnam Port → Raxaul → Birgunj", Distance = "1450 km" },
                new TransitRoute { Name = "Kolkata Port → Sunauli → Bhairahawa", Distance = "950 km" });
        }

        if (!await context.BorderPoints.AnyAsync())
        {
            context.BorderPoints.AddRange(
                new BorderPoint { Name = "Raxaul – Birgunj", State = "Bihar / Parsa" },
                new BorderPoint { Name = "Jogbani – Biratnagar", State = "Bihar / Morang" },
                new BorderPoint { Name = "Panitanki – Kakarbhitta", State = "West Bengal / Jhapa" },
                new BorderPoint { Name = "Sunauli – Bhairahawa", State = "UP / Rupandehi" },
                new BorderPoint { Name = "Rupaidiha – Nepalgunj", State = "UP / Banke" });
        }

        if (!await context.CustomsHouses.AnyAsync())
        {
            context.CustomsHouses.AddRange(
                new CustomsHouse { Name = "ICD Kolkata (Custom House)", Code = "INCCU" },
                new CustomsHouse { Name = "LCS Raxaul", Code = "INRXL" },
                new CustomsHouse { Name = "LCS Jogbani", Code = "INJOG" },
                new CustomsHouse { Name = "Custom House Visakhapatnam", Code = "INVTZ" },
                new CustomsHouse { Name = "ICD Patparganj, Delhi", Code = "INPPG" });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedSettingsAsync(AppDbContext context)
    {
        if (await context.AppSettings.AnyAsync()) return;
        context.AppSettings.Add(new AppSettingsEntity());
        await context.SaveChangesAsync();
    }

    private static async Task SeedAlertRulesAsync(AppDbContext context)
    {
        if (await context.AlertRules.AnyAsync()) return;

        context.AlertRules.AddRange(
            new AlertRule { Name = "CTD Approved Notification", Channel = AlertChannel.Email, Trigger = "Job status changes to Approved", Audience = "Operations team + Importer", Active = true },
            new AlertRule { Name = "Job Delivered", Channel = AlertChannel.EmailAndSms, Trigger = "Job status changes to Delivered", Audience = "Importer + Accounts team", Active = true },
            new AlertRule { Name = "Pending CTD Reminder", Channel = AlertChannel.Email, Trigger = "Job remains in Draft/Submitted > 3 days", Audience = "Operations team", Active = true },
            new AlertRule { Name = "Payment Overdue", Channel = AlertChannel.Sms, Trigger = "Billing status Unpaid 7+ days after Delivered", Audience = "Accounts team", Active = true });

        await context.SaveChangesAsync();
    }
}
