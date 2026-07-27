using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;

namespace CTD_FINAL.Interfaces;

public interface IDeliveryIsneService
{
    Task<DeliveryIsne?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<DeliveryIsne> SaveAsync(DeliveryIsne record, string userName, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, string userName, CancellationToken ct = default);
    Task<PagedResult<DeliveryIsne>> SearchAsync(DeliveryIsneFilter filter, int page, int pageSize, CancellationToken ct = default);
}
