using CTD_FINAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CTD_FINAL.Data.Configurations;

public class CtdJobConfiguration : IEntityTypeConfiguration<CtdJob>
{
    public void Configure(EntityTypeBuilder<CtdJob> b)
    {
        b.HasIndex(x => x.JobNo).IsUnique();
        b.HasIndex(x => x.CtdNumber);
        b.HasIndex(x => x.Status);
        b.HasIndex(x => x.JobDate);

        b.Property(x => x.InvoiceValue).HasPrecision(18, 2);
        b.Property(x => x.GrossWt).HasPrecision(18, 3);
        b.Property(x => x.NetWt).HasPrecision(18, 3);
        b.Property(x => x.ServiceCharge).HasPrecision(18, 2);
        b.Property(x => x.TransportCharge).HasPrecision(18, 2);
        b.Property(x => x.OtherCharge).HasPrecision(18, 2);
        b.Property(x => x.TaxPercent).HasPrecision(5, 2);
        b.Property(x => x.Subtotal).HasPrecision(18, 2);
        b.Property(x => x.Tax).HasPrecision(18, 2);
        b.Property(x => x.Total).HasPrecision(18, 2);

        // Importer/Agent/Transporter are three independent FKs onto the same unified Party
        // table (a party may play more than one role); no inverse Jobs collection on Party
        // since a single "Jobs" navigation would be ambiguous across three relationships.
        b.HasOne(x => x.Importer).WithMany().HasForeignKey(x => x.ImporterId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Agent).WithMany().HasForeignKey(x => x.AgentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Transporter).WithMany().HasForeignKey(x => x.TransporterId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.BorderPoint).WithMany(bp => bp.Jobs).HasForeignKey(x => x.BorderPointId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Commodity).WithMany(c => c.Jobs).HasForeignKey(x => x.CommodityId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CustomsHouse).WithMany(c => c.Jobs).HasForeignKey(x => x.CustomsHouseId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.TransitRoute).WithMany(r => r.Jobs).HasForeignKey(x => x.TransitRouteId).OnDelete(DeleteBehavior.Restrict);

        b.HasMany(x => x.Containers).WithOne(c => c.CtdJob).HasForeignKey(c => c.CtdJobId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(x => x.ChecklistItems).WithOne(c => c.CtdJob).HasForeignKey(c => c.CtdJobId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class JobContainerConfiguration : IEntityTypeConfiguration<JobContainer>
{
    public void Configure(EntityTypeBuilder<JobContainer> b)
    {
        b.Property(x => x.Weight).HasPrecision(18, 3);
        b.HasIndex(x => x.ContainerNo);
    }
}

public class GeneratedDocumentConfiguration : IEntityTypeConfiguration<GeneratedDocument>
{
    public void Configure(EntityTypeBuilder<GeneratedDocument> b)
    {
        b.HasOne(x => x.CtdJob).WithMany().HasForeignKey(x => x.CtdJobId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => x.JobNo);
        b.HasIndex(x => x.Type);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.HasIndex(x => x.JobNo);
        b.HasIndex(x => x.User);
        b.HasIndex(x => x.Timestamp);
    }
}

public class AlertLogConfiguration : IEntityTypeConfiguration<AlertLog>
{
    public void Configure(EntityTypeBuilder<AlertLog> b)
    {
        b.HasOne(x => x.AlertRule).WithMany(r => r.Logs).HasForeignKey(x => x.AlertRuleId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(x => x.SentAt);
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> b)
    {
        b.HasIndex(x => new { x.Role, x.ModuleKey }).IsUnique();
    }
}

public class NumberSequenceConfiguration : IEntityTypeConfiguration<NumberSequence>
{
    public void Configure(EntityTypeBuilder<NumberSequence> b)
    {
        b.HasIndex(x => x.Key).IsUnique();
    }
}

public class JobIsneConfiguration : IEntityTypeConfiguration<JobIsne>
{
    public void Configure(EntityTypeBuilder<JobIsne> b)
    {
        b.HasIndex(x => x.JobNumber).IsUnique();

        b.Property(x => x.ExchangeRate).HasPrecision(18, 4);
        b.Property(x => x.FobValue).HasPrecision(18, 2);
        b.Property(x => x.Freight).HasPrecision(18, 2);
        b.Property(x => x.CifFc).HasPrecision(18, 2);
        b.Property(x => x.CifFcReference).HasPrecision(18, 2);
        b.Property(x => x.InsuranceFc).HasPrecision(18, 2);
        b.Property(x => x.InsuranceValue).HasPrecision(18, 2);
        b.Property(x => x.InsuranceExRate).HasPrecision(18, 4);
        b.Property(x => x.InsuranceRate).HasPrecision(18, 3);
        b.Property(x => x.InsuranceValueInr).HasPrecision(18, 2);
        b.Property(x => x.CifInr).HasPrecision(18, 2);
        b.Property(x => x.MarketRate).HasPrecision(18, 2);
        b.Property(x => x.MarketValueInr).HasPrecision(18, 2);
        b.Property(x => x.GrossWeight).HasPrecision(18, 3);
        b.Property(x => x.NetWeight).HasPrecision(18, 3);
        b.Property(x => x.LcAmount).HasPrecision(18, 2);
        b.Property(x => x.DutyAmount).HasPrecision(18, 2);
        b.Property(x => x.SensitiveCifValue).HasPrecision(18, 2);

        b.HasMany(x => x.Containers).WithOne(c => c.JobIsne).HasForeignKey(c => c.JobIsneId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class JobIsneContainerConfiguration : IEntityTypeConfiguration<JobIsneContainer>
{
    public void Configure(EntityTypeBuilder<JobIsneContainer> b)
    {
        b.Property(x => x.GrossWeight).HasPrecision(18, 3);
        b.Property(x => x.NetWeight).HasPrecision(18, 3);
        b.HasIndex(x => x.JobIsneId);
        b.HasIndex(x => x.ContainerNo);
    }
}
