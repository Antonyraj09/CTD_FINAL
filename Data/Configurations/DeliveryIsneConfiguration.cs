using CTD_FINAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CTD_FINAL.Data.Configurations;

public class DeliveryIsneConfiguration : IEntityTypeConfiguration<DeliveryIsne>
{
    public void Configure(EntityTypeBuilder<DeliveryIsne> b)
    {
        // Fast lookup by Serial No. and Job No. (performance requirement); Deleted is part of
        // every list query's WHERE clause so it's indexed too rather than forcing a table scan.
        b.HasIndex(x => x.SerialNo).IsUnique();
        b.HasIndex(x => x.JobNo);
        b.HasIndex(x => x.DeliveryDate);
        b.HasIndex(x => x.Deleted);

        b.Property(x => x.Package).HasPrecision(18, 3);

        // Restrict (not Cascade): a Job ISNE with delivery history attached must not be
        // silently deletable — same convention CtdJob's master FKs use.
        b.HasOne(x => x.JobIsne).WithMany().HasForeignKey(x => x.JobIsneId).OnDelete(DeleteBehavior.Restrict);
    }
}
