using CTD_FINAL.Entities;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

public class NumberSequenceService : INumberSequenceService
{
    private readonly AppDbContext _context;

    public NumberSequenceService(AppDbContext context) => _context = context;

    public async Task<string> NextJobNumberAsync(CancellationToken ct = default)
    {
        var settings = await GetSettingsAsync(ct);
        var seq = await NextAsync("JobNo", ct);
        return $"{settings.JobNumberPrefix}-{DateTime.UtcNow.Year}-{seq:D4}";
    }

    public async Task<string> NextInvoiceNumberAsync(CancellationToken ct = default)
    {
        var settings = await GetSettingsAsync(ct);
        var seq = await NextAsync("InvoiceNo", ct);
        return $"{settings.InvoicePrefix}-{40000 + seq}";
    }

    public async Task<string> NextDocNumberAsync(string prefix, CancellationToken ct = default)
    {
        var seq = await NextAsync("DocNo", ct);
        return $"{prefix}-{8800 + seq}";
    }

    public async Task<string> NextIsneJobNumberAsync(CancellationToken ct = default)
    {
        var seq = await NextAsync("IsneJobNo", ct);
        return $"ISNE/{seq:D4}/{DateTime.UtcNow.Year}";
    }

    private async Task<int> NextAsync(string key, CancellationToken ct)
    {
        var row = await _context.NumberSequences.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (row is null)
        {
            row = new NumberSequence { Key = key, CurrentValue = 0 };
            _context.NumberSequences.Add(row);
        }
        row.CurrentValue += 1;
        row.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return row.CurrentValue;
    }

    private async Task<AppSettingsEntity> GetSettingsAsync(CancellationToken ct)
    {
        var settings = await _context.AppSettings.AsNoTracking().FirstOrDefaultAsync(ct);
        return settings ?? new AppSettingsEntity();
    }
}
