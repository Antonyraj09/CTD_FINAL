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
            // Minimal seed: legal name, role(s) and one head-office branch (city/phone/email/GSTIN
            // moved there — GST registration is per state in India, so it belongs on the branch, not
            // the party). PAN/IEC/CIN/banking/AEO are left blank for real data entry via the UI.
            static Party Seed(string name, string city, string phone, string email, string? gstin = null,
                bool importer = false, bool agent = false, bool transporter = false, string? license = null, string? fleet = null) => new()
            {
                Name = name, IsImporter = importer, IsAgent = agent, IsTransporter = transporter,
                License = license, Fleet = fleet,
                Branches = new List<PartyBranch>
                {
                    new() { BranchName = "Head Office", IsPrimary = true, IsActive = true, AddressLine1 = city, City = city, Country = "India", Gstin = gstin, Phone = phone, Email = email }
                }
            };

            context.Parties.AddRange(
                // Importers
                Seed("Himalaya Steel Traders Pvt Ltd", "Patna", "+91 98230 11234", "ops@himalayasteel.com", "10AABCH5566D1Z3", importer: true),
                Seed("Annapurna Foods Imports", "Raxaul", "+91 99110 44521", "contact@annapurnafoods.in", "22AAACA1234B1Z9", importer: true),
                Seed("Everest Polymers Pvt Ltd", "Kolkata", "+91 98301 22390", "logistics@everestpolymers.com", "07AAFCE7788K1Z2", importer: true),
                Seed("Kathmandu Auto Parts Co.", "Kolkata", "+91 98740 55210", "info@kathmanduauto.com.np", "19AAGCK9090M1Z1", importer: true),
                Seed("Gorkha Cement Industries", "Patna", "+91 94310 88761", "procurement@gorkhacement.com", "10AAJCG4455L1Z6", importer: true),
                Seed("Lumbini Pharma Distributors", "Gurugram", "+91 98100 67321", "supply@lumbinipharma.in", "06AAKCL2233N1Z4", importer: true),

                // Agents (CHA)
                Seed("Patel CHA Services", "Kolkata", "+91 98300 12233", "patel.cha@logimail.com", agent: true, license: "CHA/KOL/0091"),
                Seed("Eastern Border Clearing Agency", "Raxaul", "+91 94300 99887", "ebca@logimail.com", agent: true, license: "CHA/RXL/0042"),
                Seed("Siliguri Transit Consultants", "Siliguri", "+91 98765 44120", "siliguri.tc@logimail.com", agent: true, license: "CHA/SIL/0117"),
                // Also runs its own fleet across the border — demonstrates a party holding more than one role.
                Seed("Birgunj Forwarding Associates", "Birgunj (Nepal liaison)", "+977 98410 23456", "bfa@logimail.com", agent: true, transporter: true, license: "CHA/BGJ/0205", fleet: "12 Trailers"),

                // Transporters
                Seed("Himgiri Road Carriers", "Kolkata", "+91 98300 55678", "fleet@himgiritransport.com", transporter: true, fleet: "42 Trailers"),
                Seed("National Highway Logistics Pvt Ltd", "Patna", "+91 99551 23410", "ops@nhlogistics.in", transporter: true, fleet: "68 Trailers"),
                Seed("Saptkoshi Carriers", "Raxaul", "+91 94300 77654", "contact@saptkoshicarriers.com", transporter: true, fleet: "25 Trailers"),
                Seed("Konkan Rail-Road Movers", "Kolkata", "+91 98301 99001", "krrm@logimail.com", transporter: true, fleet: "Rail Rake + 30 Trailers"));
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
