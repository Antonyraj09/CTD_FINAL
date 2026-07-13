using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;

namespace CTD_FINAL.Interfaces;

public interface IJobIsneService
{
    Task<JobIsne?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<string> PeekNextJobNumberAsync(CancellationToken ct = default);
    Task<JobIsne> SaveAsync(JobIsne record, string userName, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, string userName, CancellationToken ct = default);
    Task<PagedResult<JobIsne>> SearchAsync(JobIsneTrackingFilter filter, int page, int pageSize, CancellationToken ct = default);
}
