using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

/// <summary>
/// Backs the CTD Job Wizard and Tracking screens. Field-for-field mirror of
/// the prototype's gatherJobFromForm()/saveJob()/deriveWorkflowStatus()/
/// calcBilling()/triggerAutoAlert(), moved server-side so it's real business
/// logic rather than client JS.
/// </summary>
public class JobService : IJobService
{
    private static readonly string[] ChecklistTemplate =
    {
        "Commercial Invoice", "Packing List", "Bill of Lading / LR Copy", "Bill of Entry (Transit)",
        "Certificate of Origin", "Importer Declaration", "Insurance Certificate", "Letter of Credit (if applicable)",
        "Transit Bond / Bank Guarantee", "Container Seal Verification Slip"
    };

    private readonly AppDbContext _context;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly IAuditService _auditService;
    private readonly IAlertService _alertService;

    public JobService(AppDbContext context, INumberSequenceService numberSequenceService, IAuditService auditService, IAlertService alertService)
    {
        _context = context;
        _numberSequenceService = numberSequenceService;
        _auditService = auditService;
        _alertService = alertService;
    }

    public Task<IReadOnlyList<JobChecklistItemDto>> GetDefaultChecklistAsync() =>
        Task.FromResult<IReadOnlyList<JobChecklistItemDto>>(ChecklistTemplate.Select(n => new JobChecklistItemDto { Name = n, Done = false }).ToList());

    public async Task<CtdJob?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.CtdJobs
            .Include(j => j.Importer).Include(j => j.Agent).Include(j => j.Transporter)
            .Include(j => j.BorderPoint).Include(j => j.Commodity).Include(j => j.CustomsHouse).Include(j => j.TransitRoute)
            .Include(j => j.Containers).Include(j => j.ChecklistItems)
            .FirstOrDefaultAsync(j => j.Id == id, ct);

    public async Task<CtdJob> SaveAsync(CtdJob job, List<JobContainerDto> containers, List<JobChecklistItemDto> checklist,
        bool closeJob, string userName, CancellationToken ct = default)
    {
        bool isNew = job.Id == 0;
        CtdJob entity;
        WorkflowStatus prevStatus = WorkflowStatus.Draft;

        // Server always recomputes billing — never trust client-submitted totals.
        var subtotal = job.ServiceCharge + job.TransportCharge + job.OtherCharge;
        var tax = Math.Round(subtotal * job.TaxPercent / 100, 2);
        var total = subtotal + tax;

        if (isNew)
        {
            job.JobNo = await _numberSequenceService.NextJobNumberAsync(ct);
            job.CreatedBy = userName;
            job.Status = WorkflowStatus.Draft;
            entity = job;
            _context.CtdJobs.Add(entity);
        }
        else
        {
            entity = await _context.CtdJobs.Include(j => j.Containers).Include(j => j.ChecklistItems)
                .FirstAsync(j => j.Id == job.Id, ct);
            prevStatus = entity.Status;
            CopyScalarFields(entity, job);
        }

        entity.Subtotal = subtotal;
        entity.Tax = tax;
        entity.Total = total;

        _context.JobContainers.RemoveRange(entity.Containers);
        entity.Containers = containers.Select(c => new JobContainer { ContainerNo = c.ContainerNo, Size = c.Size, Seal = c.Seal, Weight = c.Weight }).ToList();

        _context.JobChecklistItems.RemoveRange(entity.ChecklistItems);
        entity.ChecklistItems = checklist.Select((c, i) => new JobChecklistItem { Name = c.Name, Done = c.Done, SortOrder = i }).ToList();

        entity.Status = closeJob ? WorkflowStatus.Closed : DeriveWorkflowStatus(entity, isNew ? WorkflowStatus.Draft : prevStatus);
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);

        if (isNew)
        {
            await _auditService.LogAsync(AuditActionType.Created, userName, entity.JobNo, entity.Id, detail: "Job created in system", ct: ct);
        }
        else
        {
            if (entity.Status != prevStatus)
            {
                await _auditService.LogAsync(AuditActionType.StatusChange, userName, entity.JobNo, entity.Id,
                    field: "Status", fromValue: prevStatus.ToString(), toValue: entity.Status.ToString(), ct: ct);
                await TriggerAutoAlertAsync(entity, prevStatus, ct);
            }
            await _auditService.LogAsync(AuditActionType.Updated, userName, entity.JobNo, entity.Id, detail: "Job details updated", ct: ct);
        }

        return await GetByIdAsync(entity.Id, ct) ?? entity;
    }

    private static void CopyScalarFields(CtdJob target, CtdJob source)
    {
        target.JobDate = source.JobDate;
        target.ImporterId = source.ImporterId;
        target.AgentId = source.AgentId;
        target.TransporterId = source.TransporterId;
        target.OriginCountry = source.OriginCountry;
        target.PortArrival = source.PortArrival;
        target.BorderPointId = source.BorderPointId;
        target.ShipmentType = source.ShipmentType;
        target.Remarks = source.Remarks;

        target.InvoiceNo = source.InvoiceNo;
        target.InvoiceDate = source.InvoiceDate;
        target.Currency = source.Currency;
        target.InvoiceValue = source.InvoiceValue;
        target.CommodityId = source.CommodityId;
        target.HsCode = source.HsCode;
        target.GrossWt = source.GrossWt;
        target.NetWt = source.NetWt;
        target.Packages = source.Packages;

        target.CtdType = source.CtdType;
        target.CtdNumber = source.CtdNumber;
        target.CtdDate = source.CtdDate;
        target.CustomsHouseId = source.CustomsHouseId;
        target.TransitRouteId = source.TransitRouteId;
        target.ExpDeliveryDate = source.ExpDeliveryDate;

        target.ArrivalDate = source.ArrivalDate;
        target.DeliveryDate = source.DeliveryDate;
        target.DeliveryStatus = source.DeliveryStatus;
        target.ServiceCharge = source.ServiceCharge;
        target.TransportCharge = source.TransportCharge;
        target.OtherCharge = source.OtherCharge;
        target.TaxPercent = source.TaxPercent;
        target.BillingStatus = source.BillingStatus;
    }

    private static WorkflowStatus DeriveWorkflowStatus(CtdJob job, WorkflowStatus existingStatus)
    {
        if (existingStatus == WorkflowStatus.Closed) return WorkflowStatus.Closed;
        if (job.DeliveryStatus == DeliveryStatusType.Delivered) return WorkflowStatus.Delivered;
        if (job.ArrivalDate.HasValue) return WorkflowStatus.Transit;
        if (!string.IsNullOrEmpty(job.CtdNumber) && job.CtdDate.HasValue)
            return existingStatus == WorkflowStatus.Draft ? WorkflowStatus.Approved : existingStatus;
        if (job.ImporterId.HasValue && job.TransporterId.HasValue) return WorkflowStatus.Submitted;
        return WorkflowStatus.Draft;
    }

    private async Task TriggerAutoAlertAsync(CtdJob job, WorkflowStatus prevStatus, CancellationToken ct)
    {
        var importerName = job.Importer?.Name ?? (await _context.Importers.FindAsync(new object?[] { job.ImporterId }, ct))?.Name ?? "Importer";

        if (!string.IsNullOrEmpty(job.CtdNumber) && prevStatus != WorkflowStatus.Approved && job.Status == WorkflowStatus.Approved)
            await _alertService.NotifyAsync(AlertChannel.Email, importerName, "CTD Generated", job.JobNo, ct);
        if (job.Status == WorkflowStatus.Transit)
            await _alertService.NotifyAsync(AlertChannel.Sms, importerName, "Arrived at Border", job.JobNo, ct);
        if (job.Status == WorkflowStatus.Delivered)
            await _alertService.NotifyAsync(AlertChannel.EmailAndSms, importerName, "Delivery Completed", job.JobNo, ct);
    }

    public async Task<PagedResult<CtdJob>> SearchAsync(TrackingFilter filter, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.CtdJobs.AsNoTracking()
            .Include(j => j.Importer).Include(j => j.BorderPoint).Include(j => j.Containers)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.JobNo))
            query = query.Where(j => j.JobNo.Contains(filter.JobNo));
        if (!string.IsNullOrWhiteSpace(filter.CtdNo))
            query = query.Where(j => j.CtdNumber != null && j.CtdNumber.Contains(filter.CtdNo));
        if (filter.ImporterId.HasValue)
            query = query.Where(j => j.ImporterId == filter.ImporterId);
        if (!string.IsNullOrWhiteSpace(filter.Container))
            query = query.Where(j => j.Containers.Any(c => c.ContainerNo.Contains(filter.Container)));
        if (filter.DateFrom.HasValue)
            query = query.Where(j => j.JobDate >= filter.DateFrom);
        if (filter.DateTo.HasValue)
            query = query.Where(j => j.JobDate <= filter.DateTo);
        if (!string.IsNullOrWhiteSpace(filter.Status) && Enum.TryParse<WorkflowStatus>(filter.Status, out var status))
            query = query.Where(j => j.Status == status);
        if (!string.IsNullOrWhiteSpace(filter.Quick))
        {
            var q = filter.Quick;
            query = query.Where(j => j.JobNo.Contains(q) || (j.CtdNumber != null && j.CtdNumber.Contains(q))
                || (j.Importer != null && j.Importer.Name.Contains(q)) || (j.BorderPoint != null && j.BorderPoint.Name.Contains(q))
                || j.Containers.Any(c => c.ContainerNo.Contains(q)));
        }

        query = (filter.SortKey, filter.SortDir) switch
        {
            ("jobNo", "asc") => query.OrderBy(j => j.JobNo),
            ("jobNo", _) => query.OrderByDescending(j => j.JobNo),
            ("importer", "asc") => query.OrderBy(j => j.Importer!.Name),
            ("importer", _) => query.OrderByDescending(j => j.Importer!.Name),
            ("status", "asc") => query.OrderBy(j => j.Status),
            ("status", _) => query.OrderByDescending(j => j.Status),
            ("jobDate", "asc") => query.OrderBy(j => j.JobDate),
            _ => query.OrderByDescending(j => j.JobDate),
        };

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<CtdJob> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<CtdJob?> MarkDocumentGeneratedAsync(int jobId, string docType, CancellationToken ct = default)
    {
        var job = await _context.CtdJobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job is null) return null;

        switch (docType)
        {
            case "CTD Document": job.CtdDocGenerated = true; break;
            case "Checklist": job.ChecklistDocGenerated = true; break;
            case "Forwarding Note": job.ForwardingDocGenerated = true; break;
        }
        await _context.SaveChangesAsync(ct);
        return job;
    }

    public async Task<CtdJob?> MarkInvoiceGeneratedAsync(int jobId, string userName, CancellationToken ct = default)
    {
        var job = await _context.CtdJobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job is null) return null;

        job.InvoiceGenerated = true;
        if (job.BillingStatus == BillingStatus.Unpaid) job.BillingStatus = BillingStatus.Partial;
        await _context.SaveChangesAsync(ct);

        await _auditService.LogAsync(AuditActionType.DocumentGenerated, userName, job.JobNo, job.Id, detail: "Invoice generated", ct: ct);
        return job;
    }
}
