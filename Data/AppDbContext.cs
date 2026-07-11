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
    public DbSet<Commodity> Commodities => Set<Commodity>();
    public DbSet<TransitRoute> TransitRoutes => Set<TransitRoute>();
    public DbSet<BorderPoint> BorderPoints => Set<BorderPoint>();
    public DbSet<CustomsHouse> CustomsHouses => Set<CustomsHouse>();

    public DbSet<CtdJob> CtdJobs => Set<CtdJob>();
    public DbSet<JobContainer> JobContainers => Set<JobContainer>();
    public DbSet<JobChecklistItem> JobChecklistItems => Set<JobChecklistItem>();
    public DbSet<JobIsne> JobIsnes => Set<JobIsne>();

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
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Identity tables: keep default AspNetXxx names but trim to our simplified schema footprint.
        builder.Entity<ApplicationUser>(b => b.ToTable("AspNetUsers"));
        builder.Entity<ApplicationRole>(b => b.ToTable("AspNetRoles"));
    }
}
