using CTD_FINAL.DTOs;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

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
        "deliveryStatus" => await DeliveryStatusAsync(ct),
        "customerRevenue" => await CustomerRevenueAsync(ct),
        "billingSummary" => await BillingSummaryAsync(ct),
        _ => null
    };

    private IQueryable<CTD_FINAL.Entities.CtdJob> BaseQuery() =>
        _context.CtdJobs.AsNoTracking()
            .Include(j => j.Importer).Include(j => j.Transporter).Include(j => j.BorderPoint)
            .Include(j => j.CustomsHouse).Include(j => j.Containers);

    private async Task<ReportResult> DailyJobAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var jobs = await BaseQuery().Where(j => j.JobDate.Date == today).OrderByDescending(j => j.JobDate).ToListAsync(ct);
        if (jobs.Count == 0)
            jobs = await BaseQuery().OrderByDescending(j => j.JobDate).Take(12).ToListAsync(ct);

        return new ReportResult
        {
            Title = "Daily Job Report",
            Columns = new[] { "Job No.", "Date", "Importer", "Transporter", "Border Point", "Status" },
            Rows = jobs.Select(j => new[]
            {
                j.JobNo, j.JobDate.ToString("d MMM yyyy"), j.Importer?.Name ?? "—", j.Transporter?.Name ?? "—",
                j.BorderPoint?.Name ?? "—", j.Status.ToString()
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
            Columns = new[] { "Job No.", "CTD No.", "CTD Type", "CTD Date", "Importer", "Customs House" },
            Rows = jobs.Select(j => new[]
            {
                j.JobNo, j.CtdNumber ?? "—", j.CtdType.ToString(), j.CtdDate?.ToString("d MMM yyyy") ?? "—",
                j.Importer?.Name ?? "—", j.CustomsHouse?.Name ?? "—"
            }).ToList()
        };
    }

    private async Task<ReportResult> PendingCtdAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var jobs = await BaseQuery()
            .Where(j => j.Status == WorkflowStatus.Draft || j.Status == WorkflowStatus.Submitted)
            .OrderBy(j => j.JobDate).ToListAsync(ct);

        return new ReportResult
        {
            Title = "Pending CTD Report",
            Columns = new[] { "Job No.", "Date", "Importer", "Days Pending", "Status" },
            Rows = jobs.Select(j => new[]
            {
                j.JobNo, j.JobDate.ToString("d MMM yyyy"), j.Importer?.Name ?? "—",
                $"{Math.Max(0, (today - j.JobDate.Date).Days)} days", j.Status.ToString()
            }).ToList()
        };
    }

    private async Task<ReportResult> ContainerMovementAsync(CancellationToken ct)
    {
        var jobs = await BaseQuery().OrderByDescending(j => j.JobDate).ToListAsync(ct);
        var rows = jobs.SelectMany(j => j.Containers.Select(c => new[]
        {
            c.ContainerNo, c.Size ?? "—", c.Seal ?? "—", c.Weight.ToString("N0"), j.JobNo, j.Status.ToString()
        })).ToList();

        return new ReportResult
        {
            Title = "Container Movement Report",
            Columns = new[] { "Container No.", "Size", "Seal", "Weight (kg)", "Job No.", "Status" },
            Rows = rows
        };
    }

    private async Task<ReportResult> DeliveryStatusAsync(CancellationToken ct)
    {
        var jobs = await BaseQuery().OrderByDescending(j => j.JobDate).ToListAsync(ct);

        return new ReportResult
        {
            Title = "Delivery Status Report",
            Columns = new[] { "Job No.", "Importer", "Border Point", "Arrival Date", "Delivery Date", "Delivery Status" },
            Rows = jobs.Select(j => new[]
            {
                j.JobNo, j.Importer?.Name ?? "—", j.BorderPoint?.Name ?? "—",
                j.ArrivalDate?.ToString("d MMM yyyy") ?? "—", j.DeliveryDate?.ToString("d MMM yyyy") ?? "—",
                j.DeliveryStatus.ToString()
            }).ToList()
        };
    }

    private async Task<ReportResult> CustomerRevenueAsync(CancellationToken ct)
    {
        var jobs = await BaseQuery().ToListAsync(ct);
        var grouped = jobs.GroupBy(j => j.Importer?.Name ?? "—")
            .Select(g => new
            {
                Name = g.Key,
                Jobs = g.Count(),
                Revenue = g.Sum(j => j.Total),
                Outstanding = g.Where(j => j.BillingStatus != BillingStatus.Paid).Sum(j => j.Total)
            })
            .OrderByDescending(g => g.Revenue).ToList();

        return new ReportResult
        {
            Title = "Customer-wise Revenue Report",
            Columns = new[] { "Importer", "Total Jobs", "Total Revenue", "Outstanding" },
            Rows = grouped.Select(g => new[]
            {
                g.Name, g.Jobs.ToString(), $"₹{g.Revenue:N2}", $"₹{g.Outstanding:N2}"
            }).ToList()
        };
    }

    private async Task<ReportResult> BillingSummaryAsync(CancellationToken ct)
    {
        var jobs = await BaseQuery().OrderByDescending(j => j.JobDate).ToListAsync(ct);

        return new ReportResult
        {
            Title = "Billing Summary Report",
            Columns = new[] { "Job No.", "Service", "Transport", "Other", "Tax", "Total", "Payment Status" },
            Rows = jobs.Select(j => new[]
            {
                j.JobNo, $"₹{j.ServiceCharge:N2}", $"₹{j.TransportCharge:N2}", $"₹{j.OtherCharge:N2}",
                $"₹{j.Tax:N2}", $"₹{j.Total:N2}", j.BillingStatus.ToString()
            }).ToList()
        };
    }
}
