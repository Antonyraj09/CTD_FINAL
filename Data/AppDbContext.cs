using CTD_FINAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Party> Parties => Set<Party>();
    public DbSet<PartyBranch> PartyBranches => Set<PartyBranch>();
    public DbSet<Commodity> Commodities => Set<Commodity>();
    public DbSet<TransitRoute> TransitRoutes => Set<TransitRoute>();
    public DbSet<BorderPoint> BorderPoints => Set<BorderPoint>();
    public DbSet<CustomsHouse> CustomsHouses => Set<CustomsHouse>();
    public DbSet<SubAgent> SubAgents => Set<SubAgent>();

    public DbSet<CtdJob> CtdJobs => Set<CtdJob>();
    public DbSet<JobContainer> JobContainers => Set<JobContainer>();
    public DbSet<JobChecklistItem> JobChecklistItems => Set<JobChecklistItem>();
    public DbSet<JobIsne> JobIsnes => Set<JobIsne>();
    public DbSet<JobIsneContainer> JobIsneContainers => Set<JobIsneContainer>();

    public DbSet<GeneratedDocument> GeneratedDocuments => Set<GeneratedDocument>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<AlertLog> AlertLogs => Set<AlertLog>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<NumberSequence> NumberSequences => Set<NumberSequence>();
    public DbSet<AppSettingsEntity> AppSettings => Set<AppSettingsEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Excludes Data/Configurations/AdminConfigurations.cs — those configure ADMIN_CTD-only
        // entities (Company, License, ClientDatabase, InstallationHistory, AdminNumberSequence)
        // that must never appear in a tenant's own database. ApplyConfigurationsFromAssembly has
        // no built-in namespace filter, so this predicate checks each configuration's target
        // entity type directly (the same leak AdminDbContext avoids by listing its 5 configs
        // explicitly instead — not practical here given AppDbContext's much larger config count).
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly, IsTenantEntityConfiguration);

        // Identity tables: keep default AspNetXxx names but trim to our simplified schema footprint.
        builder.Entity<ApplicationUser>(b => b.ToTable("AspNetUsers"));
        builder.Entity<ApplicationRole>(b => b.ToTable("AspNetRoles"));
    }

    private static bool IsTenantEntityConfiguration(Type configurationType) =>
        !configurationType.GetInterfaces().Any(i =>
            i.IsGenericType &&
            i.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>) &&
            i.GenericTypeArguments[0].Namespace == "CTD_FINAL.Entities.Admin");
}
