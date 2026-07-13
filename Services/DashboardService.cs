using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Helpers;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

/// <summary>
/// Reads from JobIsne, not CtdJob — JobIsne has no WorkflowStatus/BillingStatus/BorderPoint FK,
/// so every figure here is an honest derivation from the fields JobIsne actually has:
/// see Helpers/JobIsneStatus.cs for the 3-state pseudo-status, RouteOfTransit (free text)
/// stands in for border point, and DutyAmount/FobValue/CifInr stand in for revenue/billing.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context) => _context = context;

    public async Task<DashboardKpiDto> GetKpisAsync(CancellationToken ct = default)
    {
        var jobs = _context.JobIsnes.AsNoTracking();

        return new DashboardKpiDto
        {
            Total = await jobs.CountAsync(ct),
            CtdIssued = await jobs.CountAsync(JobIsneStatus.IsCtdIssued, ct),
            Arrived = await jobs.CountAsync(JobIsneStatus.IsArrived, ct),
            PendingCtd = await jobs.CountAsync(JobIsneStatus.IsPendingCtd, ct),
            GreenCtdCount = await jobs.CountAsync(j => j.GreenCtd, ct),
            TotalDuty = await jobs.SumAsync(j => (decimal?)j.DutyAmount, ct) ?? 0
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
        var jobs = await _context.JobIsnes.AsNoTracking()
            .Where(j => j.JobDate >= earliest)
            .Select(j => new { j.JobDate, j.DutyAmount })
            .ToListAsync(ct);

        return buckets.Select(b => new MonthlyPointDto
        {
            Label = b.Label,
            Count = jobs.Count(j => j.JobDate.Year == b.Year && j.JobDate.Month == b.Month),
            TotalDuty = jobs.Where(j => j.JobDate.Year == b.Year && j.JobDate.Month == b.Month).Sum(j => j.DutyAmount ?? 0)
        }).ToList();
    }

    public async Task<IReadOnlyList<StatusCountDto>> GetStatusDistributionAsync(CancellationToken ct = default)
    {
        var jobs = _context.JobIsnes.AsNoTracking();
        return new List<StatusCountDto>
        {
            new() { Status = JobIsneStatus.PendingCtd, Count = await jobs.CountAsync(JobIsneStatus.IsPendingCtd, ct) },
            new() { Status = JobIsneStatus.CtdIssued, Count = await jobs.CountAsync(JobIsneStatus.IsCtdIssued, ct) },
            new() { Status = JobIsneStatus.Arrived, Count = await jobs.CountAsync(JobIsneStatus.IsArrived, ct) },
        };
    }

    public async Task<IReadOnlyList<RouteCountDto>> GetRouteVolumeAsync(CancellationToken ct = default)
    {
        var counts = await _context.JobIsnes.AsNoTracking()
            .GroupBy(j => j.RouteOfTransit)
            .Select(g => new { Route = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(6)
            .ToListAsync(ct);

        return counts.Select(c => new RouteCountDto { Name = string.IsNullOrWhiteSpace(c.Route) ? "Unspecified" : c.Route!, Count = c.Count }).ToList();
    }

    public async Task<IReadOnlyList<DashboardAlertDto>> GetAlertsAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var alerts = new List<DashboardAlertDto>();

        var pendingCtd = await _context.JobIsnes.AsNoTracking()
            .Where(JobIsneStatus.IsPendingCtd)
            .OrderByDescending(j => j.JobDate)
            .Take(4)
            .ToListAsync(ct);
        alerts.AddRange(pendingCtd.Select(j => new DashboardAlertDto
        {
            Type = "warning",
            Title = $"CTD pending — {j.JobNumber}",
            Sub = $"{j.PartyName} · filed {j.JobDate:d MMM yyyy}"
        }));

        var overdueDocs = pendingCtd
            .Select(j => new
            {
                Job = j,
                Earliest = new[] { j.DuePackingList, j.DueInvoice, j.DueOriginalBl, j.DueInsuranceCert, j.DueLcCopy }
                    .Where(d => d.HasValue && d.Value.Date < today)
                    .OrderBy(d => d)
                    .FirstOrDefault()
            })
            .Where(x => x.Earliest.HasValue)
            .OrderBy(x => x.Earliest)
            .Take(3);
        alerts.AddRange(overdueDocs.Select(x => new DashboardAlertDto
        {
            Type = "error",
            Title = $"Document overdue — {x.Job.JobNumber}",
            Sub = $"Due {x.Earliest:d MMM yyyy} · {x.Job.PartyName}"
        }));

        var expiringLc = await _context.JobIsnes.AsNoTracking()
            .Where(j => j.ShipmentExpiry != null && j.ShipmentExpiry >= today && j.ShipmentExpiry <= today.AddDays(7))
            .OrderBy(j => j.ShipmentExpiry)
            .Take(3)
            .ToListAsync(ct);
        alerts.AddRange(expiringLc.Select(j => new DashboardAlertDto
        {
            Type = "info",
            Title = $"LC shipment expiry approaching — {j.JobNumber}",
            Sub = $"Expires {j.ShipmentExpiry:d MMM yyyy} · {j.PartyName}"
        }));

        return alerts.Take(6).ToList();
    }

    public async Task<IReadOnlyList<RecentJobDto>> GetRecentJobsAsync(int count = 8, CancellationToken ct = default)
    {
        var jobs = await _context.JobIsnes.AsNoTracking()
            .OrderByDescending(j => j.JobDate)
            .Take(count)
            .ToListAsync(ct);

        return jobs.Select(j => new RecentJobDto
        {
            Id = j.Id,
            JobNo = j.JobNumber,
            JobDate = j.JobDate,
            PartyName = j.PartyName,
            CtdNumber = j.CtdNumber,
            ContainerCount = string.IsNullOrWhiteSpace(j.ContainerNo) ? 0 : 1,
            ContainerSize = j.ContainerSize,
            Route = j.RouteOfTransit,
            Status = JobIsneStatus.Label(j)
        }).ToList();
    }

    public async Task<CustomerKpiDto> GetCustomerKpisAsync(int importerId, CancellationToken ct = default)
    {
        var jobs = await JobsForImporterAsync(importerId, ct);
        return new CustomerKpiDto
        {
            TotalShipments = jobs.Count,
            CtdIssued = jobs.Count(j => JobIsneStatus.Label(j) == JobIsneStatus.CtdIssued),
            Arrived = jobs.Count(j => JobIsneStatus.Label(j) == JobIsneStatus.Arrived),
            PendingCtd = jobs.Count(j => JobIsneStatus.Label(j) == JobIsneStatus.PendingCtd)
        };
    }

    public async Task<IReadOnlyList<CustomerShipmentDto>> GetCustomerShipmentsAsync(int importerId, CancellationToken ct = default)
    {
        var jobs = await JobsForImporterAsync(importerId, ct);
        return jobs.OrderByDescending(j => j.JobDate).Take(10).Select(j => new CustomerShipmentDto
        {
            Id = j.Id,
            JobNo = j.JobNumber,
            CtdNumber = j.CtdNumber,
            Container = j.ContainerNo,
            Route = j.RouteOfTransit,
            Status = JobIsneStatus.Label(j),
            ArrivalDate = j.VesselArrival
        }).ToList();
    }

    public async Task<CustomerCommercialDto> GetCustomerCommercialAsync(int importerId, CancellationToken ct = default)
    {
        var jobs = await JobsForImporterAsync(importerId, ct);
        return new CustomerCommercialDto
        {
            TotalFobValue = jobs.Sum(j => j.FobValue ?? 0),
            TotalCifInr = jobs.Sum(j => j.CifInr ?? 0),
            TotalDuty = jobs.Sum(j => j.DutyAmount ?? 0)
        };
    }

    public async Task<IReadOnlyList<TimelineStepDto>> GetShipmentTimelineAsync(int jobId, CancellationToken ct = default)
    {
        var job = await _context.JobIsnes.AsNoTracking().FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job is null) return Array.Empty<TimelineStepDto>();

        var steps = new (string Label, DateTime? Date)[]
        {
            ("Job Created", job.JobDate),
            (!string.IsNullOrEmpty(job.CtdNumber) ? $"CTD Issued · {job.CtdNumber}" : "CTD Pending", job.CtdDate),
            ("Vessel Arrived", job.VesselArrival),
        };

        int currentIdx = string.IsNullOrEmpty(job.CtdNumber) ? 0 : (job.VesselArrival is null ? 1 : 2);
        return steps.Select((s, i) => new TimelineStepDto
        {
            Label = s.Label,
            Date = s.Date,
            CssClass = i < currentIdx ? "done" : (i == currentIdx ? "current" : "")
        }).ToList();
    }

    // JobIsne has no FK to Party — it only carries a free-text PartyName — so the
    // Customer Dashboard's importerId (a Party.Id) is bridged by matching that party's
    // legal name against JobIsne.PartyName rather than a real foreign key.
    private async Task<List<JobIsne>> JobsForImporterAsync(int importerId, CancellationToken ct)
    {
        var party = await _context.Parties.AsNoTracking().FirstOrDefaultAsync(p => p.Id == importerId, ct);
        if (party is null) return new List<JobIsne>();
        return await _context.JobIsnes.AsNoTracking().Where(j => j.PartyName == party.Name).ToListAsync(ct);
    }
}
