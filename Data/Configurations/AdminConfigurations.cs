using CTD_FINAL.Entities.Admin;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CTD_FINAL.Data.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> b)
    {
        b.HasIndex(x => x.CompanyCode).IsUnique();
        b.HasIndex(x => x.Email);
    }
}

public class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> b)
    {
        b.HasIndex(x => x.LicenseNumber).IsUnique();
        b.HasOne(x => x.Company).WithMany(c => c.Licenses).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ClientDatabaseConfiguration : IEntityTypeConfiguration<ClientDatabase>
{
    public void Configure(EntityTypeBuilder<ClientDatabase> b)
    {
        b.HasIndex(x => x.CompanyId);
        b.HasOne(x => x.Company).WithMany(c => c.ClientDatabases).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class InstallationHistoryConfiguration : IEntityTypeConfiguration<InstallationHistory>
{
    public void Configure(EntityTypeBuilder<InstallationHistory> b)
    {
        b.HasIndex(x => x.CompanyId);
        b.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class AdminNumberSequenceConfiguration : IEntityTypeConfiguration<AdminNumberSequence>
{
    public void Configure(EntityTypeBuilder<AdminNumberSequence> b)
    {
        b.HasIndex(x => x.Key).IsUnique();
    }
}
