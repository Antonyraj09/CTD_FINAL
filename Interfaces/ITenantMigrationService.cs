namespace CTD_FINAL.Interfaces;

/// <summary>Applies pending EF Core migrations to every active tenant's own database in one
/// pass, instead of running Update-Database against each client's database by hand. Backs the
/// `migrate-tenants` CLI command (see Program.cs) — reuses the same AppDbContext +
/// Database.MigrateAsync() pattern ProvisioningService already uses when provisioning a
/// brand-new tenant, which is safe to re-run because EF only ever applies migrations not yet
/// recorded in that database's own __EFMigrationsHistory table.</summary>
public interface ITenantMigrationService
{
    /// <summary>Runs against every Active tenant, printing a per-tenant report to the console,
    /// and returns the process exit code (0 if every tenant succeeded or was already current,
    /// 1 if any tenant failed). When dryRun is true, nothing is applied — pending migrations
    /// are only listed.</summary>
    Task<int> RunCliAsync(bool dryRun, CancellationToken ct = default);
}
