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

    /// <summary>Unlike the other Next*Async methods, this is computed live from the JobIsnes
    /// table (MAX existing sequence + 1) rather than a persisted NumberSequences counter row —
    /// deletes are hard deletes here, so the next number should always reflect what's actually
    /// left, e.g. deleting the highest-numbered job frees its number back up for the next one.</summary>
    public async Task<string> NextIsneJobNumberAsync(CancellationToken ct = default)
    {
        var existingNumbers = await _context.JobIsnes.AsNoTracking().Select(j => j.JobNumber).ToListAsync(ct);
        var next = existingNumbers.Select(ParseIsneSequence).DefaultIfEmpty(0).Max() + 1;
        return $"ISNE/{next:D4}/{DateTime.UtcNow.Year}";
    }

    private static int ParseIsneSequence(string jobNumber)
    {
        var parts = jobNumber.Split('/');
        return parts.Length >= 2 && int.TryParse(parts[1], out var seq) ? seq : 0;
    }

    public Task<int> NextDeliveryIsneSerialAsync(CancellationToken ct = default) => NextAsync("DeliveryIsneSerial", ct);

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
