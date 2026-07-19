using CTD_FINAL.Data;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CTD_FINAL.Services;

public class TenantResolutionService : ITenantResolutionService
{
    private const string CacheKeyPrefix = "TenantResolution:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly AdminDbContext _adminContext;
    private readonly IEncryptionService _encryptionService;
    private readonly IMemoryCache _cache;

    public TenantResolutionService(AdminDbContext adminContext, IEncryptionService encryptionService, IMemoryCache cache)
    {
        _adminContext = adminContext;
        _encryptionService = encryptionService;
        _cache = cache;
    }

    public async Task<TenantResolution> ResolveAsync(string licenseNumber, CancellationToken ct = default)
    {
        var cacheKey = CacheKeyPrefix + licenseNumber;
        if (_cache.TryGetValue(cacheKey, out TenantResolution? cached) && cached is not null)
            return cached;

        var resolution = await ResolveUncachedAsync(licenseNumber, ct);

        // Only cache successful resolutions — a not-found/invalid license shouldn't stick
        // around in cache and block a retry once the underlying row is fixed/created.
        if (resolution.Found && resolution.LicenseValid)
            _cache.Set(cacheKey, resolution, CacheDuration);

        return resolution;
    }

    public void Invalidate(string licenseNumber) => _cache.Remove(CacheKeyPrefix + licenseNumber);

    private async Task<TenantResolution> ResolveUncachedAsync(string licenseNumber, CancellationToken ct)
    {
        var license = await _adminContext.Licenses.AsNoTracking()
            .Include(l => l.Company)
            .FirstOrDefaultAsync(l => l.LicenseNumber == licenseNumber, ct);

        if (license is null)
            return new TenantResolution(false, null, 0, string.Empty, false, "License not found.");

        if (license.Status != LicenseStatus.Active)
            return new TenantResolution(true, null, license.CompanyId, license.Company.CompanyName, false, "License is not active.");

        if (license.ExpiryDate < DateTime.UtcNow)
            return new TenantResolution(true, null, license.CompanyId, license.Company.CompanyName, false, "License has expired.");

        var clientDb = await _adminContext.ClientDatabases.AsNoTracking()
            .Where(d => d.CompanyId == license.CompanyId && d.Status == ClientDatabaseStatus.Active)
            .FirstOrDefaultAsync(ct);

        if (clientDb is null)
            return new TenantResolution(true, null, license.CompanyId, license.Company.CompanyName, false, "No active database is mapped to this license.");

        string connectionString;
        try
        {
            connectionString = _encryptionService.Decrypt(clientDb.EncryptedConnectionString);
        }
        catch (Exception)
        {
            return new TenantResolution(true, null, license.CompanyId, license.Company.CompanyName, false, "Could not decrypt the tenant's database connection.");
        }

        return new TenantResolution(true, connectionString, license.CompanyId, license.Company.CompanyName, true, null);
    }
}
