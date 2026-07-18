using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CTD_FINAL.Data.Seed;

/// <summary>
/// Boot-time setup for the fixed ADMIN_CTD database — migrate only. Unlike the tenant-side
/// seeding (which now only ever runs once, during Install Wizard provisioning of a brand-new
/// client database — see TenantSeeder), ADMIN_CTD has no default rows of its own: it starts
/// genuinely empty and gets its first Company/License/ClientDatabase rows the moment someone
/// completes the Install Wizard.
/// </summary>
public static class AdminDbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<AdminDbContext>();
        await context.Database.MigrateAsync();
    }
}
