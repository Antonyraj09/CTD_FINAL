using CTD_FINAL.Entities;

namespace CTD_FINAL.Interfaces;

public interface ISettingsService
{
    Task<AppSettingsEntity> GetAsync(CancellationToken ct = default);
    Task<AppSettingsEntity> SaveAsync(AppSettingsEntity settings, CancellationToken ct = default);
}
