using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;

namespace CTD_FINAL.Interfaces;

public interface IJobService
{
    Task<CtdJob?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<CtdJob> SaveAsync(CtdJob job, List<JobContainerDto> containers, List<JobChecklistItemDto> checklist,
        bool closeJob, string userName, CancellationToken ct = default);
    Task<PagedResult<CtdJob>> SearchAsync(TrackingFilter filter, int page, int pageSize, CancellationToken ct = default);
    Task<CtdJob?> MarkDocumentGeneratedAsync(int jobId, string docType, CancellationToken ct = default);
    Task<CtdJob?> MarkInvoiceGeneratedAsync(int jobId, string userName, CancellationToken ct = default);
    Task<IReadOnlyList<JobChecklistItemDto>> GetDefaultChecklistAsync();
}
