using CTD_FINAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CTD_FINAL.Data.Configurations;

public class ImporterConfiguration : IEntityTypeConfiguration<Importer>
{
    public void Configure(EntityTypeBuilder<Importer> b)
    {
        b.HasIndex(x => x.Gstin).IsUnique();
        b.HasIndex(x => x.Name);
    }
}

public class AgentConfiguration : IEntityTypeConfiguration<Agent>
{
    public void Configure(EntityTypeBuilder<Agent> b)
    {
        b.HasIndex(x => x.License).IsUnique();
        b.HasIndex(x => x.Name);
    }
}

public class TransporterConfiguration : IEntityTypeConfiguration<Transporter>
{
    public void Configure(EntityTypeBuilder<Transporter> b)
    {
        b.HasIndex(x => x.Name);
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
