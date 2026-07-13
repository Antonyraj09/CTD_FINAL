using CTD_FINAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CTD_FINAL.Data.Configurations;

public class PartyConfiguration : IEntityTypeConfiguration<Party>
{
    public void Configure(EntityTypeBuilder<Party> b)
    {
        // Unique indexes tolerate many NULLs (SQL Server treats each NULL as distinct),
        // so rows that aren't tagged Importer/Agent leave Pan/License blank safely.
        b.HasIndex(x => x.PartyCode).IsUnique();
        b.HasIndex(x => x.Pan).IsUnique();
        b.HasIndex(x => x.IecCode).IsUnique();
        b.HasIndex(x => x.CinNumber).IsUnique();
        b.HasIndex(x => x.License).IsUnique();
        b.HasIndex(x => x.Name);

        b.HasMany(x => x.Branches).WithOne(br => br.Party).HasForeignKey(br => br.PartyId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class PartyBranchConfiguration : IEntityTypeConfiguration<PartyBranch>
{
    public void Configure(EntityTypeBuilder<PartyBranch> b)
    {
        // Same GSTIN can't legitimately appear on two different branches (even across parties).
        b.HasIndex(x => x.Gstin).IsUnique();
        b.HasIndex(x => x.PartyId);
    }
}

public class CommodityConfiguration : IEntityTypeConfiguration<Commodity>
{
    public void Configure(EntityTypeBuilder<Commodity> b)
    {
        b.HasIndex(x => x.HsCode);
        b.HasIndex(x => x.Name);
    }
}

public class TransitRouteConfiguration : IEntityTypeConfiguration<TransitRoute>
{
    public void Configure(EntityTypeBuilder<TransitRoute> b)
    {
        b.HasIndex(x => x.Name);
    }
}

public class BorderPointConfiguration : IEntityTypeConfiguration<BorderPoint>
{
    public void Configure(EntityTypeBuilder<BorderPoint> b)
    {
        b.HasIndex(x => x.Name);
    }
}

public class CustomsHouseConfiguration : IEntityTypeConfiguration<CustomsHouse>
{
    public void Configure(EntityTypeBuilder<CustomsHouse> b)
    {
        b.HasIndex(x => x.Code).IsUnique();
        b.HasIndex(x => x.Name);
    }
}

public class SubAgentConfiguration : IEntityTypeConfiguration<SubAgent>
{
    public void Configure(EntityTypeBuilder<SubAgent> b)
    {
        b.HasIndex(x => x.SubAgentCode).IsUnique();
        b.HasIndex(x => x.Name);
    }
}
