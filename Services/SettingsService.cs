using CTD_FINAL.Entities;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

public class SettingsService : ISettingsService
{
    private readonly AppDbContext _context;

    public SettingsService(AppDbContext context) => _context = context;

    public async Task<AppSettingsEntity> GetAsync(CancellationToken ct = default)
    {
        var settings = await _context.AppSettings.AsNoTracking().FirstOrDefaultAsync(ct);
        return settings ?? new AppSettingsEntity();
    }

    public async Task<AppSettingsEntity> SaveAsync(AppSettingsEntity settings, CancellationToken ct = default)
    {
        var existing = await _context.AppSettings.FirstOrDefaultAsync(ct);
        if (existing is null)
        {
            _context.AppSettings.Add(settings);
        }
        else
        {
            existing.JobNumberPrefix = settings.JobNumberPrefix;
            existing.InvoicePrefix = settings.InvoicePrefix;
            existing.DocumentPrefix = settings.DocumentPrefix;
            existing.CompanyName = settings.CompanyName;
            existing.CompanyAddress = settings.CompanyAddress;
            existing.CompanyGstin = settings.CompanyGstin;
            existing.ChaLicenseNo = settings.ChaLicenseNo;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync(ct);
        return existing ?? settings;
    }
}
