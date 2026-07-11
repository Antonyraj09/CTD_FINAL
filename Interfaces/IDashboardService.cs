using CTD_FINAL.DTOs;

namespace CTD_FINAL.Interfaces;

public interface IDashboardService
{
    Task<DashboardKpiDto> GetKpisAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MonthlyPointDto>> GetMonthlyAggregateAsync(int months = 6, CancellationToken ct = default);
    Task<IReadOnlyList<StatusCountDto>> GetStatusDistributionAsync(CancellationToken ct = default);
    Task<IReadOnlyList<BorderPointCountDto>> GetBorderPointVolumeAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DashboardAlertDto>> GetAlertsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<RecentJobDto>> GetRecentJobsAsync(int count = 8, CancellationToken ct = default);

    Task<CustomerKpiDto> GetCustomerKpisAsync(int importerId, CancellationToken ct = default);
    Task<IReadOnlyList<CustomerShipmentDto>> GetCustomerShipmentsAsync(int importerId, CancellationToken ct = default);
    Task<CustomerBillingDto> GetCustomerBillingAsync(int importerId, CancellationToken ct = default);
    Task<IReadOnlyList<TimelineStepDto>> GetShipmentTimelineAsync(int jobId, CancellationToken ct = default);
}
