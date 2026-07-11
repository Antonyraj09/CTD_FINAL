using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditActionType action, string user, string? jobNo = null, int? jobId = null,
        string? field = null, string? fromValue = null, string? toValue = null, string? detail = null,
        CancellationToken ct = default);

    Task<PagedResult<AuditLog>> SearchAsync(string? jobNo, string? user, AuditActionType? action,
        int page, int pageSize, CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetDistinctUsersAsync(CancellationToken ct = default);
}
