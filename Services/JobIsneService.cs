using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Helpers;
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
        await _context.JobIsnes.AsNoTracking()
            .Include(j => j.Containers.OrderBy(c => c.SortOrder))
            .FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<string> PeekNextJobNumberAsync(CancellationToken ct = default)
    {
        var row = await _context.NumberSequences.AsNoTracking().FirstOrDefaultAsync(s => s.Key == "IsneJobNo", ct);
        var next = (row?.CurrentValue ?? 0) + 1;
        return $"ISNE/{next:D4}/{DateTime.UtcNow.Year}";
    }

    public async Task<JobIsne> SaveAsync(JobIsne record, List<JobIsneContainer> containers, string userName, CancellationToken ct = default)
    {
        for (var i = 0; i < containers.Count; i++) containers[i].SortOrder = i;

        if (record.Id == 0)
        {
            record.JobNumber = await _numberSequenceService.NextIsneJobNumberAsync(ct);
            record.CreatedBy = userName;
            record.Containers = containers;
            _context.JobIsnes.Add(record);
            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(AuditActionType.Created, userName, record.JobNumber, detail: "Job ISNE created");
            return record;
        }

        var existing = await _context.JobIsnes.Include(j => j.Containers).FirstOrDefaultAsync(j => j.Id == record.Id, ct)
            ?? throw new InvalidOperationException($"Job ISNE #{record.Id} not found.");

        record.JobNumber = existing.JobNumber;
        record.CreatedBy = existing.CreatedBy;
        _context.Entry(existing).CurrentValues.SetValues(record);

        _context.JobIsneContainers.RemoveRange(existing.Containers);
        existing.Containers.Clear();
        foreach (var c in containers)
        {
            c.Id = 0;
            existing.Containers.Add(c);
        }

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

    public async Task<PagedResult<JobIsne>> SearchAsync(JobIsneTrackingFilter filter, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.JobIsnes.AsNoTracking().Include(j => j.Containers).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.JobNo))
            query = query.Where(j => j.JobNumber.Contains(filter.JobNo));
        if (!string.IsNullOrWhiteSpace(filter.CtdNo))
            query = query.Where(j => j.CtdNumber != null && j.CtdNumber.Contains(filter.CtdNo));
        if (!string.IsNullOrWhiteSpace(filter.PartyName))
            query = query.Where(j => j.PartyName.Contains(filter.PartyName));
        if (!string.IsNullOrWhiteSpace(filter.Container))
            query = query.Where(j => j.Containers.Any(c => c.ContainerNo != null && c.ContainerNo.Contains(filter.Container)));
        if (filter.DateFrom.HasValue)
            query = query.Where(j => j.JobDate >= filter.DateFrom);
        if (filter.DateTo.HasValue)
            query = query.Where(j => j.JobDate <= filter.DateTo);
        query = filter.Status switch
        {
            JobIsneStatus.PendingCtd => query.Where(JobIsneStatus.IsPendingCtd),
            JobIsneStatus.CtdIssued => query.Where(JobIsneStatus.IsCtdIssued),
            JobIsneStatus.Arrived => query.Where(JobIsneStatus.IsArrived),
            _ => query
        };
        if (!string.IsNullOrWhiteSpace(filter.Quick))
        {
            var q = filter.Quick;
            query = query.Where(j => j.JobNumber.Contains(q) || j.PartyName.Contains(q)
                || (j.CtdNumber != null && j.CtdNumber.Contains(q))
                || j.Containers.Any(c => c.ContainerNo != null && c.ContainerNo.Contains(q))
                || (j.RouteOfTransit != null && j.RouteOfTransit.Contains(q)));
        }

        query = (filter.SortKey, filter.SortDir) switch
        {
            ("jobNo", "asc") => query.OrderBy(j => j.JobNumber),
            ("jobNo", _) => query.OrderByDescending(j => j.JobNumber),
            ("party", "asc") => query.OrderBy(j => j.PartyName),
            ("party", _) => query.OrderByDescending(j => j.PartyName),
            ("jobDate", "asc") => query.OrderBy(j => j.JobDate),
            _ => query.OrderByDescending(j => j.JobDate),
        };

        return await query.ToPagedResultAsync(page, pageSize, ct);
    }
}
