using CTD_FINAL.Data.Configurations;
using CTD_FINAL.Entities.Admin;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Data;

/// <summary>
/// The fixed, always-reachable ADMIN_CTD database — one row per licensed company, license,
/// client database mapping, and installation attempt. Unlike AppDbContext (whose connection
/// string is resolved dynamically per request via ITenantContextAccessor), this context's
/// connection is a single static ConnectionStrings:AdminConnection, since it IS the thing
/// every other tenant lookup starts from.
///
/// Deliberately does NOT call ApplyConfigurationsFromAssembly (unlike AppDbContext) — that
/// scans the whole assembly for any IEntityTypeConfiguration&lt;T&gt;, which would pull every
/// tenant-side entity (Party, CtdJob, JobIsne, ...) into this model too. Admin configurations
/// are applied explicitly instead.
/// </summary>
public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<ClientDatabase> ClientDatabases => Set<ClientDatabase>();
    public DbSet<InstallationHistory> InstallationHistories => Set<InstallationHistory>();
    public DbSet<AdminNumberSequence> NumberSequences => Set<AdminNumberSequence>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new CompanyConfiguration());
        builder.ApplyConfiguration(new LicenseConfiguration());
        builder.ApplyConfiguration(new ClientDatabaseConfiguration());
        builder.ApplyConfiguration(new InstallationHistoryConfiguration());
        builder.ApplyConfiguration(new AdminNumberSequenceConfiguration());
    }
}
