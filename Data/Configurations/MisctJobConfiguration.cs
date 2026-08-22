using CTD_FINAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CTD_FINAL.Data.Configurations;

public class MisctJobConfiguration : IEntityTypeConfiguration<MisctJob>
{
    public void Configure(EntityTypeBuilder<MisctJob> b)
    {
        b.HasIndex(x => x.JobNo).IsUnique();

        b.Property(x => x.GrossWeight).HasPrecision(18, 3);

        b.HasOne(x => x.CustomsStationExit).WithMany().HasForeignKey(x => x.CustomsStationExitId).OnDelete(DeleteBehavior.SetNull);
        b.HasOne(x => x.PortOfEntryNepal).WithMany().HasForeignKey(x => x.PortOfEntryNepalId).OnDelete(DeleteBehavior.SetNull);

        b.HasMany(x => x.Containers).WithOne(c => c.MisctJob).HasForeignKey(c => c.MisctJobId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class MisctJobContainerConfiguration : IEntityTypeConfiguration<MisctJobContainer>
{
    public void Configure(EntityTypeBuilder<MisctJobContainer> b)
    {
        b.Property(x => x.Weight).HasPrecision(18, 3);
        b.Property(x => x.CifValue).HasPrecision(18, 2);
        b.HasIndex(x => x.MisctJobId);
        b.HasIndex(x => x.ContainerNo);
    }
}
