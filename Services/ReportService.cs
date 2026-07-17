using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Helpers;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

/// <summary>
/// Reads from JobIsne, not CtdJob. JobIsne has no WorkflowStatus/BillingStatus/BorderPoint
/// FK/ServiceCharge-Transport-Tax fields, so reports that depended on those are repurposed
/// around what JobIsne actually has — see Helpers/JobIsneStatus.cs for the pseudo-status,
/// and the Commercial Value Summary report (formerly Billing Summary) below.
/// </summary>
public class ReportService : IReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context) => _context = context;

    public async Task<ReportResult?> GetReportAsync(string key, CancellationToken ct = default) => key switch
    {
        "dailyJob" => await DailyJobAsync(ct),
        "ctdRegister" => await CtdRegisterAsync(ct),
        "pendingCtd" => await PendingCtdAsync(ct),
        "containerMovement" => await ContainerMovementAsync(ct),
        "deliveryStatus" => await ArrivalStatusAsync(ct),
        "customerRevenue" => await CustomerDutyAsync(ct),
        "billingSummary" => await CommercialValueSummaryAsync(ct),
        _ => null
    };

    private IQueryable<JobIsne> BaseQuery() => _context.JobIsnes.AsNoTracking().Include(j => j.Containers);

    private static string CustomsCodesOf(JobIsne j) =>
        string.Join(", ", j.Containers.Select(c => c.CustomsCode).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct());

    private async Task<ReportResult> DailyJobAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var jobs = await BaseQuery().Where(j => j.JobDate.Date == today).OrderByDescending(j => j.JobDate).ToListAsync(ct);
        if (jobs.Count == 0)
            jobs = await BaseQuery().OrderByDescending(j => j.JobDate).Take(12).ToListAsync(ct);

        return new ReportResult
        {
            Title = "Daily Job Report",
            Columns = new[] { "Job No.", "Date", "Party", "Sub-Agent", "Route of Transit", "Status" },
            Rows = jobs.Select(j => new[]
            {
                j.JobNumber, j.JobDate.ToString("d MMM yyyy"), j.PartyName, j.SubAgentName ?? "—",
                j.RouteOfTransit ?? "—", JobIsneStatus.Label(j)
            }).ToList()
        };
    }

    private async Task<ReportResult> CtdRegisterAsync(CancellationToken ct)
    {
        var jobs = await BaseQuery().Where(j => j.CtdNumber != null && j.CtdNumber != "")
            .OrderByDescending(j => j.CtdDate).ToListAsync(ct);

        return new ReportResult
        {
            Title = "CTD Register",
            Columns = new[] { "Job No.", "CTD No.", "CTD Date", "Party", "Customs Code", "Green CTD" },
            Rows = jobs.Select(j => new[]
            {
                j.JobNumber, j.CtdNumber ?? "—", j.CtdDate?.ToString("d MMM yyyy") ?? "—",
                j.PartyName, CustomsCodesOf(j) is var codes && codes.Length > 0 ? codes : "—", j.GreenCtd ? "Yes" : "No"
            }).ToList()
        };
    }

    private async Task<ReportResult> PendingCtdAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var jobs = await BaseQuery().Where(JobIsneStatus.IsPendingCtd).OrderBy(j => j.JobDate).ToListAsync(ct);

        return new ReportResult
        {
            Title = "Pending CTD Report",
            Columns = new[] { "Job No.", "Date", "Party", "Days Pending", "Status" },
            Rows = jobs.Select(j => new[]
            {
                j.JobNumber, j.JobDate.ToString("d MMM yyyy"), j.PartyName,
                $"{Math.Max(0, (today - j.JobDate.Date).Days)} days", JobIsneStatus.Label(j)
            }).ToList()
        };
    }

    private async Task<ReportResult> ContainerMovementAsync(CancellationToken ct)
    {
        var jobs = await BaseQuery().Where(j => j.Containers.Any(c => c.ContainerNo != null && c.ContainerNo != ""))
            .OrderByDescending(j => j.JobDate).ToListAsync(ct);

        return new ReportResult
        {
            Title = "Container Movement Report",
            Columns = new[] { "Container No.", "Size", "Status Type", "Gross Weight (kg)", "Job No.", "Status" },
            Rows = jobs.SelectMany(j => j.Containers
                .Where(c => !string.IsNullOrEmpty(c.ContainerNo))
                .Select(c => new[]
                {
                    c.ContainerNo!, c.ContainerSize, c.ShipmentType.ToString(),
                    c.GrossWeight?.ToString("N0") ?? "—", j.JobNumber, JobIsneStatus.Label(j)
                })).ToList()
        };
    }

    private async Task<ReportResult> ArrivalStatusAsync(CancellationToken ct)
    {
        var jobs = await BaseQuery().OrderByDescending(j => j.JobDate).ToListAsync(ct);

        return new ReportResult
        {
            Title = "Arrival Status Report",
            Columns = new[] { "Job No.", "Party", "Route of Transit", "Vessel Arrival", "CTD Sent To", "Status" },
            Rows = jobs.Select(j => new[]
            {
                j.JobNumber, j.PartyName, j.RouteOfTransit ?? "—",
                j.VesselArrival?.ToString("d MMM yyyy") ?? "—", j.CtdSentTo ?? "—", JobIsneStatus.Label(j)
            }).ToList()
        };
    }

    private async Task<ReportResult> CustomerDutyAsync(CancellationToken ct)
    {
        var jobs = await BaseQuery().ToListAsync(ct);
        var grouped = jobs.GroupBy(j => j.PartyName)
            .Select(g => new
            {
                Name = g.Key,
                Jobs = g.Count(),
                TotalDuty = g.Sum(j => j.DutyAmount ?? 0),
                PendingDuty = g.Where(j => string.IsNullOrEmpty(j.CtdNumber)).Sum(j => j.DutyAmount ?? 0)
            })
            .OrderByDescending(g => g.TotalDuty).ToList();

        return new ReportResult
        {
            Title = "Customer-wise Duty Report",
            Columns = new[] { "Party", "Total Jobs", "Total Duty", "Duty Pending CTD" },
            Rows = grouped.Select(g => new[]
            {
                g.Name, g.Jobs.ToString(), $"₹{g.TotalDuty:N2}", $"₹{g.PendingDuty:N2}"
            }).ToList()
        };
    }

    private async Task<ReportResult> CommercialValueSummaryAsync(CancellationToken ct)
    {
        var jobs = await BaseQuery().OrderByDescending(j => j.JobDate).ToListAsync(ct);

        return new ReportResult
        {
            Title = "Commercial Value Summary",
            Columns = new[] { "Job No.", "FOB Value", "Freight", "CIF (₹)", "Duty Amount", "Green CTD" },
            Rows = jobs.Select(j => new[]
            {
                j.JobNumber,
                j.FobValue.HasValue ? $"{j.Currency} {j.FobValue:N2}" : "—",
                j.Freight.HasValue ? $"{j.Currency} {j.Freight:N2}" : "—",
                j.CifInr.HasValue ? $"₹{j.CifInr:N2}" : "—",
                j.DutyAmount.HasValue ? $"₹{j.DutyAmount:N2}" : "—",
                j.GreenCtd ? "Yes" : "No"
            }).ToList()
        };
    }
}
