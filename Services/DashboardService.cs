using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

public class DashboardService : IDashboardService
{
    private static readonly WorkflowStatus[] ActiveStatuses = { WorkflowStatus.Submitted, WorkflowStatus.Approved, WorkflowStatus.Transit };
    private static readonly WorkflowStatus[] DeliveredStatuses = { WorkflowStatus.Delivered, WorkflowStatus.Closed };

    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context) => _context = context;

    public async Task<DashboardKpiDto> GetKpisAsync(CancellationToken ct = default)
    {
        var jobs = _context.CtdJobs.AsNoTracking();
        var today = DateTime.UtcNow.Date;

        return new DashboardKpiDto
        {
            Total = await jobs.CountAsync(ct),
            Active = await jobs.CountAsync(j => ActiveStatuses.Contains(j.Status), ct),
            Delivered = await jobs.CountAsync(j => DeliveredStatuses.Contains(j.Status), ct),
            PendingCtd = await jobs.CountAsync(j => j.Status == WorkflowStatus.Draft || (j.Status == WorkflowStatus.Submitted && j.CtdNumber == null), ct),
            PendingBilling = await jobs.CountAsync(j => j.BillingStatus != BillingStatus.Paid, ct),
            Revenue = await jobs.SumAsync(j => (decimal?)j.Total, ct) ?? 0
        };
    }

    public async Task<IReadOnlyList<MonthlyPointDto>> GetMonthlyAggregateAsync(int months = 6, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow;
        var buckets = new List<(int Year, int Month, string Label)>();
        for (int i = months - 1; i >= 0; i--)
        {
            var d = today.AddMonths(-i);
            buckets.Add((d.Year, d.Month, d.ToString("MMM")));
        }

        var earliest = new DateTime(buckets[0].Year, buckets[0].Month, 1);
        var jobs = await _context.CtdJobs.AsNoTracking()
            .Where(j => j.JobDate >= earliest)
            .Select(j => new { j.JobDate, j.Total })
            .ToListAsync(ct);

        return buckets.Select(b => new MonthlyPointDto
        {
            Label = b.Label,
            Count = jobs.Count(j => j.JobDate.Year == b.Year && j.JobDate.Month == b.Month),
            Revenue = jobs.Where(j => j.JobDate.Year == b.Year && j.JobDate.Month == b.Month).Sum(j => j.Total)
        }).ToList();
    }

    public async Task<IReadOnlyList<StatusCountDto>> GetStatusDistributionAsync(CancellationToken ct = default)
    {
        var counts = await _context.CtdJobs.AsNoTracking()
            .GroupBy(j => j.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return Enum.GetValues<WorkflowStatus>()
            .Select(s => new StatusCountDto { Status = s.ToString(), Count = counts.FirstOrDefault(c => c.Status == s)?.Count ?? 0 })
            .ToList();
    }

    public async Task<IReadOnlyList<BorderPointCountDto>> GetBorderPointVolumeAsync(CancellationToken ct = default)
    {
        var borderPoints = await _context.BorderPoints.AsNoTracking().ToListAsync(ct);
        var counts = await _context.CtdJobs.AsNoTracking()
            .Where(j => j.BorderPointId != null)
            .GroupBy(j => j.BorderPointId)
            .Select(g => new { BorderPointId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        return borderPoints.Select(b => new BorderPointCountDto
        {
            Name = b.Name,
            Count = counts.FirstOrDefault(c => c.BorderPointId == b.Id)?.Count ?? 0
        }).ToList();
    }

    public async Task<IReadOnlyList<DashboardAlertDto>> GetAlertsAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var alerts = new List<DashboardAlertDto>();

        var pendingCtd = await _context.CtdJobs.AsNoTracking()
            .Include(j => j.Importer)
            .Where(j => j.Status == WorkflowStatus.Draft || j.Status == WorkflowStatus.Submitted)
            .OrderByDescending(j => j.JobDate)
            .Take(4)
            .ToListAsync(ct);
        alerts.AddRange(pendingCtd.Select(j => new DashboardAlertDto
        {
            Type = "warning",
            Title = $"CTD pending — {j.JobNo}",
            Sub = $"{j.Importer?.Name} · filed {j.JobDate:d MMM yyyy}"
        }));

        var overdue = await _context.CtdJobs.AsNoTracking()
            .Where(j => j.ExpDeliveryDate != null && j.Status != WorkflowStatus.Delivered && j.Status != WorkflowStatus.Closed && j.ExpDeliveryDate < today)
            .Take(3)
            .ToListAsync(ct);
        alerts.AddRange(overdue.Select(j => new DashboardAlertDto
        {
            Type = "error",
            Title = $"Delivery overdue — {j.JobNo}",
            Sub = $"Expected {j.ExpDeliveryDate:d MMM yyyy} · {j.BorderPoint?.Name}"
        }));

        var unpaid = await _context.CtdJobs.AsNoTracking()
            .Include(j => j.Importer)
            .Where(j => j.BillingStatus == BillingStatus.Unpaid && (j.Status == WorkflowStatus.Delivered || j.Status == WorkflowStatus.Closed))
            .Take(3)
            .ToListAsync(ct);
        alerts.AddRange(unpaid.Select(j => new DashboardAlertDto
        {
            Type = "info",
            Title = $"Billing pending — {j.JobNo}",
            Sub = $"₹{j.Total:N2} due from {j.Importer?.Name}"
        }));

        return alerts.Take(6).ToList();
    }

    public async Task<IReadOnlyList<RecentJobDto>> GetRecentJobsAsync(int count = 8, CancellationToken ct = default)
    {
        var jobs = await _context.CtdJobs.AsNoTracking()
            .Include(j => j.Importer)
            .Include(j => j.BorderPoint)
            .Include(j => j.Containers)
            .OrderByDescending(j => j.JobDate)
            .Take(count)
            .ToListAsync(ct);

        return jobs.Select(j => new RecentJobDto
        {
            Id = j.Id,
            JobNo = j.JobNo,
            JobDate = j.JobDate,
            ImporterName = j.Importer?.Name ?? "Unassigned",
            CtdNumber = j.CtdNumber,
            ContainerCount = j.Containers.Count,
            ContainerSize = j.Containers.FirstOrDefault()?.Size,
            BorderPoint = j.BorderPoint?.Name,
            Status = j.Status.ToString(),
            BillingStatus = j.BillingStatus.ToString()
        }).ToList();
    }

    public async Task<CustomerKpiDto> GetCustomerKpisAsync(int importerId, CancellationToken ct = default)
    {
        var jobs = _context.CtdJobs.AsNoTracking().Where(j => j.ImporterId == importerId);
        return new CustomerKpiDto
        {
            TotalShipments = await jobs.CountAsync(ct),
            InTransit = await jobs.CountAsync(j => ActiveStatuses.Contains(j.Status), ct),
            Delivered = await jobs.CountAsync(j => DeliveredStatuses.Contains(j.Status), ct),
            OutstandingInvoices = await jobs.CountAsync(j => j.BillingStatus != BillingStatus.Paid, ct)
        };
    }

    public async Task<IReadOnlyList<CustomerShipmentDto>> GetCustomerShipmentsAsync(int importerId, CancellationToken ct = default)
    {
        var jobs = await _context.CtdJobs.AsNoTracking()
            .Include(j => j.BorderPoint)
            .Include(j => j.Containers)
            .Where(j => j.ImporterId == importerId && j.Status != WorkflowStatus.Closed)
            .OrderByDescending(j => j.JobDate)
            .Take(10)
            .ToListAsync(ct);

        return jobs.Select(j => new CustomerShipmentDto
        {
            Id = j.Id,
            JobNo = j.JobNo,
            CtdNumber = j.CtdNumber,
            Containers = string.Join(", ", j.Containers.Select(c => c.ContainerNo)),
            BorderPoint = j.BorderPoint?.Name,
            Status = j.Status.ToString(),
            ExpDeliveryDate = j.ExpDeliveryDate
        }).ToList();
    }

    public async Task<CustomerBillingDto> GetCustomerBillingAsync(int importerId, CancellationToken ct = default)
    {
        var jobs = await _context.CtdJobs.AsNoTracking().Where(j => j.ImporterId == importerId).ToListAsync(ct);
        var billed = jobs.Sum(j => j.Total);
        var paid = jobs.Where(j => j.BillingStatus == BillingStatus.Paid).Sum(j => j.Total);
        return new CustomerBillingDto { TotalBilled = billed, TotalPaid = paid, Outstanding = billed - paid };
    }

    public async Task<IReadOnlyList<TimelineStepDto>> GetShipmentTimelineAsync(int jobId, CancellationToken ct = default)
    {
        var job = await _context.CtdJobs.AsNoTracking().Include(j => j.BorderPoint).FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job is null) return Array.Empty<TimelineStepDto>();

        var steps = new (WorkflowStatus Key, string Label, DateTime? Date)[]
        {
            (WorkflowStatus.Draft, "Job Created", job.JobDate),
            (WorkflowStatus.Submitted, "Submitted to Customs", job.CtdDate),
            (WorkflowStatus.Approved, $"CTD Approved · {job.CtdNumber ?? "Pending"}", job.CtdDate),
            (WorkflowStatus.Transit, $"In Transit · {job.BorderPoint?.Name}", job.ArrivalDate),
            (WorkflowStatus.Delivered, "Delivered in Nepal", job.DeliveryDate),
            (WorkflowStatus.Closed, "Job Closed & Billed", job.Status == WorkflowStatus.Closed ? job.UpdatedAt : null),
        };

        int currentIdx = (int)job.Status;
        return steps.Select(s =>
        {
            int stepIdx = (int)s.Key;
            var cls = stepIdx < currentIdx ? "done" : (stepIdx == currentIdx ? "current" : "");
            return new TimelineStepDto { Label = s.Label, Date = s.Date, CssClass = cls };
        }).ToList();
    }
}
