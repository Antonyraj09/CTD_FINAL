using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

public class JobIsneService : IJobIsneService
{
    private readonly AppDbContext _context;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly IAuditService _auditService;

    public JobIsneService(AppDbContext context, INumberSequenceService numberSequenceService, IAuditService auditService)
    {
        _context = context;
        _numberSequenceService = numberSequenceService;
        _auditService = auditService;
    }

    public async Task<JobIsne?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.JobIsnes.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<string> PeekNextJobNumberAsync(CancellationToken ct = default)
    {
        var row = await _context.NumberSequences.AsNoTracking().FirstOrDefaultAsync(s => s.Key == "IsneJobNo", ct);
        var next = (row?.CurrentValue ?? 0) + 1;
        return $"ISNE/{next:D4}/{DateTime.UtcNow.Year}";
    }

    public async Task<JobIsne> SaveAsync(JobIsne record, string userName, CancellationToken ct = default)
    {
        if (record.Id == 0)
        {
            record.JobNumber = await _numberSequenceService.NextIsneJobNumberAsync(ct);
            record.CreatedBy = userName;
            _context.JobIsnes.Add(record);
            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(AuditActionType.Created, userName, record.JobNumber, detail: "Job ISNE created");
            return record;
        }

        var existing = await _context.JobIsnes.FirstOrDefaultAsync(j => j.Id == record.Id, ct)
            ?? throw new InvalidOperationException($"Job ISNE #{record.Id} not found.");

        record.JobNumber = existing.JobNumber;
        record.CreatedBy = existing.CreatedBy;
        _context.Entry(existing).CurrentValues.SetValues(record);
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        await _auditService.LogAsync(AuditActionType.Updated, userName, existing.JobNumber, detail: "Job ISNE updated");
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, string userName, CancellationToken ct = default)
    {
        var record = await _context.JobIsnes.FirstOrDefaultAsync(j => j.Id == id, ct);
        if (record is null) return false;

        _context.JobIsnes.Remove(record);
        await _context.SaveChangesAsync(ct);
        await _auditService.LogAsync(AuditActionType.Deleted, userName, record.JobNumber, detail: "Job ISNE deleted");
        return true;
    }
}
