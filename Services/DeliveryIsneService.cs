using CTD_FINAL.Data;
using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Helpers;
using CTD_FINAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

public class DeliveryIsneService : IDeliveryIsneService
{
    private readonly AppDbContext _context;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly IAuditService _auditService;

    public DeliveryIsneService(AppDbContext context, INumberSequenceService numberSequenceService, IAuditService auditService)
    {
        _context = context;
        _numberSequenceService = numberSequenceService;
        _auditService = auditService;
    }

    public async Task<DeliveryIsne?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.DeliveryIsnes.AsNoTracking()
            .Include(d => d.JobIsne)
            .FirstOrDefaultAsync(d => d.Id == id && !d.Deleted, ct);

    public async Task<DeliveryIsne> SaveAsync(DeliveryIsne record, string userName, CancellationToken ct = default)
    {
        if (record.Id == 0)
        {
            record.SerialNo = await _numberSequenceService.NextDeliveryIsneSerialAsync(ct);
            record.CreatedBy = userName;
            _context.DeliveryIsnes.Add(record);
            await _context.SaveChangesAsync(ct);
            await _auditService.LogAsync(AuditActionType.Created, userName, record.JobNo, record.JobIsneId,
                detail: $"Delivery ISNE #{record.SerialNo} created for Job {record.JobNo}");
            return record;
        }

        var existing = await _context.DeliveryIsnes.FirstOrDefaultAsync(d => d.Id == record.Id && !d.Deleted, ct)
            ?? throw new InvalidOperationException($"Delivery ISNE #{record.Id} not found.");

        // Serial No., Job No./JobIsneId and Customer Name are read-only once created — a Delivery
        // stays tied to the Job it was raised for, and re-numbering it would break the "continues
        // from the last saved record" sequence guarantee.
        record.SerialNo = existing.SerialNo;
        record.JobIsneId = existing.JobIsneId;
        record.JobNo = existing.JobNo;
        record.CustomerName = existing.CustomerName;
        record.CreatedBy = existing.CreatedBy;
        record.Deleted = existing.Deleted;

        _context.Entry(existing).CurrentValues.SetValues(record);
        existing.ModifiedBy = userName;
        existing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        await _auditService.LogAsync(AuditActionType.Updated, userName, existing.JobNo, existing.JobIsneId,
            detail: $"Delivery ISNE #{existing.SerialNo} updated");
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, string userName, CancellationToken ct = default)
    {
        var record = await _context.DeliveryIsnes.FirstOrDefaultAsync(d => d.Id == id && !d.Deleted, ct);
        if (record is null) return false;

        record.Deleted = true;
        record.DeletedAt = DateTime.UtcNow;
        record.DeletedBy = userName;
        await _context.SaveChangesAsync(ct);
        await _auditService.LogAsync(AuditActionType.Deleted, userName, record.JobNo, record.JobIsneId,
            detail: $"Delivery ISNE #{record.SerialNo} deleted (soft)");
        return true;
    }

    public async Task<PagedResult<DeliveryIsne>> SearchAsync(DeliveryIsneFilter filter, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.DeliveryIsnes.AsNoTracking().Where(d => !d.Deleted).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.SerialNo) && int.TryParse(filter.SerialNo, out var serialNo))
            query = query.Where(d => d.SerialNo == serialNo);
        if (!string.IsNullOrWhiteSpace(filter.JobNo))
            query = query.Where(d => d.JobNo.Contains(filter.JobNo));
        if (!string.IsNullOrWhiteSpace(filter.Customer))
            query = query.Where(d => d.CustomerName != null && d.CustomerName.Contains(filter.Customer));
        if (!string.IsNullOrWhiteSpace(filter.Transporter))
            query = query.Where(d => d.TransporterName != null && d.TransporterName.Contains(filter.Transporter));
        if (filter.DateFrom.HasValue)
            query = query.Where(d => d.DeliveryDate >= filter.DateFrom);
        if (filter.DateTo.HasValue)
            query = query.Where(d => d.DeliveryDate <= filter.DateTo);
        if (!string.IsNullOrWhiteSpace(filter.Quick))
        {
            var q = filter.Quick;
            query = query.Where(d => d.JobNo.Contains(q)
                || (d.CustomerName != null && d.CustomerName.Contains(q))
                || (d.TransporterName != null && d.TransporterName.Contains(q))
                || (d.ContainerNo != null && d.ContainerNo.Contains(q))
                || (d.ConsigneeName != null && d.ConsigneeName.Contains(q)));
        }

        query = (filter.SortKey, filter.SortDir) switch
        {
            ("serialNo", "asc") => query.OrderBy(d => d.SerialNo),
            ("serialNo", _) => query.OrderByDescending(d => d.SerialNo),
            ("jobNo", "asc") => query.OrderBy(d => d.JobNo),
            ("jobNo", _) => query.OrderByDescending(d => d.JobNo),
            ("deliveryDate", "asc") => query.OrderBy(d => d.DeliveryDate),
            _ => query.OrderByDescending(d => d.DeliveryDate),
        };

        return await query.ToPagedResultAsync(page, pageSize, ct);
    }
}
