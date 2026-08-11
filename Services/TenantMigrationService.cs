using CTD_FINAL.Data;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

public class TenantMigrationService : ITenantMigrationService
{
    private readonly AdminDbContext _adminContext;
    private readonly IEncryptionService _encryptionService;

    public TenantMigrationService(AdminDbContext adminContext, IEncryptionService encryptionService)
    {
        _adminContext = adminContext;
        _encryptionService = encryptionService;
    }

    public async Task<int> RunCliAsync(bool dryRun, CancellationToken ct = default)
    {
        var tenants = await _adminContext.ClientDatabases
            .Include(d => d.Company)
            .Where(d => d.Status == ClientDatabaseStatus.Active)
            .OrderBy(d => d.Company.CompanyName)
            .ToListAsync(ct);

        if (tenants.Count == 0)
        {
            Console.WriteLine("No active tenant databases found in ADMIN_CTD.");
            return 0;
        }

        Console.WriteLine(dryRun
            ? $"Checking {tenants.Count} active tenant database(s) for pending migrations (dry run — nothing will be applied)..."
            : $"Migrating {tenants.Count} active tenant database(s)...");
        Console.WriteLine();

        var failures = 0;
        foreach (var tenant in tenants)
        {
            var label = $"{tenant.Company.CompanyName} ({tenant.DatabaseName})";
            try
            {
                var connectionString = _encryptionService.Decrypt(tenant.EncryptedConnectionString);
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                    .Options;

                await using var context = new AppDbContext(options);
                var pending = (await context.Database.GetPendingMigrationsAsync(ct)).ToList();

                if (pending.Count == 0)
                {
                    Console.WriteLine($"[CURRENT] {label} — already up to date.");
                    continue;
                }

                if (dryRun)
                {
                    Console.WriteLine($"[PENDING] {label} — {pending.Count} migration(s): {string.Join(", ", pending)}");
                    continue;
                }

                await context.Database.MigrateAsync(ct);
                Console.WriteLine($"[OK]      {label} — applied {pending.Count} migration(s): {string.Join(", ", pending)}");
            }
            catch (Exception ex)
            {
                failures++;
                Console.WriteLine($"[FAIL]    {label} — {ex.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine(failures == 0
            ? $"Done — {tenants.Count} tenant(s) processed, no failures."
            : $"Done — {tenants.Count} tenant(s) processed, {failures} failure(s). See above for details.");

        return failures == 0 ? 0 : 1;
    }
}
