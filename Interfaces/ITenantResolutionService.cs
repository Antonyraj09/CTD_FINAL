namespace CTD_FINAL.Interfaces;

public record TenantResolution(bool Found, string? ConnectionString, int CompanyId, string CompanyName, bool LicenseValid, string? FailureReason);

/// <summary>Resolves a License Number to its tenant's decrypted connection details, backed by AdminDbContext and cached (mirrors PermissionService's IMemoryCache pattern) so most authenticated page views never hit ADMIN_CTD at all.</summary>
public interface ITenantResolutionService
{
    Task<TenantResolution> ResolveAsync(string licenseNumber, CancellationToken ct = default);
    void Invalidate(string licenseNumber);
}
