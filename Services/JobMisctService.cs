using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Helpers;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

public class JobMisctService : IJobMisctService
{
    private readonly AppDbContext _context;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly IAuditService _auditService;

    public JobMisctService(AppDbContext context, INumberSequenceService numberSequenceService, IAuditService auditService)
    {
        _context = context;
        _numberSequenceService = numberSequenceService;
        _auditService = auditService;
    }

    public async Task<MisctJob?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.MisctJobs.AsNoTracking()
            .Include(j => j.Containers.OrderBy(c => c.SortOrder))
            .Include(j => j.CustomsStationExit)
            .Include(j => j.PortOfEntryNepal)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

    public Task<string> PeekNextJobNumberAsync(CancellationToken ct = default) =>
        _numberSequenceService.NextMisctJobNumberAsync(ct);

    public async Task<IReadOnlyList<MisctJob>> SearchByJobNoPrefixAsync(string prefix, CancellationToken ct = default) =>
        await _context.MisctJobs.AsNoTracking()
            .Where(j => j.JobNo.StartsWith(prefix))
            .OrderByDescending(j => j.JobNo)
            .Take(10)
            .ToListAsync(ct);

    public async Task<PagedResult<MisctJobListItem>> SearchAsync(string? query, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _context.MisctJobs.AsNoTracking().OrderByDescending(j => j.JobDate).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(j => j.JobNo.Contains(term)
                || j.PartyName.Contains(term)
                || (j.VesselName != null && j.VesselName.Contains(term))
                || (j.MblNo != null && j.MblNo.Contains(term)));
        }

        var projected = q.Select(j => new MisctJobListItem
        {
            Id = j.Id,
            JobNo = j.JobNo,
            JobDate = j.JobDate,
            PartyName = j.PartyName,
            VesselName = j.VesselName,
            VoyageNo = j.VoyageNo,
            MblNo = j.MblNo,
            GrossWeight = j.GrossWeight,
            InvoiceNo = j.InvoiceNo,
            InvoiceDate = j.InvoiceDate,
            ContainerCount = j.Containers.Count
        });

        return await projected.ToPagedResultAsync(page, pageSize, ct);
    }

    public async Task<MisctJob> SaveAsync(MisctJob record, List<MisctJobContainer> containers, string userName, CancellationToken ct = default)
    {
        for (var i = 0; i < containers.Count; i++) containers[i].SortOrder = i;

        if (record.Id == 0)
        {
            record.JobNo = await _numberSequenceService.NextMisctJobNumberAsync(ct);
            record.CreatedBy = userName;
            record.Containers = containers;
            _context.MisctJobs.Add(record);
            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(AuditActionType.Created, userName, record.JobNo, detail: "Job MISCT created");
            return record;
        }

        var existing = await _context.MisctJobs.Include(j => j.Containers).FirstOrDefaultAsync(j => j.Id == record.Id, ct)
            ?? throw new InvalidOperationException($"Job MISCT #{record.Id} not found.");

        record.JobNo = existing.JobNo;
        record.CreatedBy = existing.CreatedBy;
        _context.Entry(existing).CurrentValues.SetValues(record);

        _context.MisctJobContainers.RemoveRange(existing.Containers);
        existing.Containers.Clear();
        foreach (var c in containers)
        {
            c.Id = 0;
            existing.Containers.Add(c);
        }

        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        await _auditService.LogAsync(AuditActionType.Updated, userName, existing.JobNo, detail: "Job MISCT updated");
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, string userName, CancellationToken ct = default)
    {
        var record = await _context.MisctJobs.FirstOrDefaultAsync(j => j.Id == id, ct);
        if (record is null) return false;

        _context.MisctJobs.Remove(record);
        await _context.SaveChangesAsync(ct);
        await _auditService.LogAsync(AuditActionType.Deleted, userName, record.JobNo, detail: "Job MISCT deleted");
        return true;
    }
}
