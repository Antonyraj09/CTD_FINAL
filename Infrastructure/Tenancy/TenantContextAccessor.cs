using CTD_FINAL.Interfaces;

namespace CTD_FINAL.Infrastructure.Tenancy;

/// <summary>Plain scoped implementation — one instance per HTTP request (registered AddScoped), holding whatever tenant was resolved for this request.</summary>
public class TenantContextAccessor : ITenantContextAccessor
{
    public string? ConnectionString { get; private set; }
    public string? LicenseNumber { get; private set; }
    public int? CompanyId { get; private set; }
    public string? CompanyName { get; private set; }

    public void Set(string connectionString, string licenseNumber, int companyId, string companyName)
    {
        ConnectionString = connectionString;
        LicenseNumber = licenseNumber;
        CompanyId = companyId;
        CompanyName = companyName;
    }
}
