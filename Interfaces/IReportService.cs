using CTD_FINAL.DTOs;

namespace CTD_FINAL.Interfaces;

public interface IReportService
{
    /// <summary>Report keys: dailyJob, ctdRegister, pendingCtd, containerMovement, deliveryStatus, customerRevenue, billingSummary.</summary>
    Task<ReportResult?> GetReportAsync(string key, CancellationToken ct = default);
}
