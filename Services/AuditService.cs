using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context) => _context = context;

    public async Task LogAsync(AuditActionType action, string user, string? jobNo = null, int? jobId = null,
        string? field = null, string? fromValue = null, string? toValue = null, string? detail = null,
        CancellationToken ct = default)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            Action = action,
            User = user,
            JobNo = jobNo,
            JobId = jobId,
            Field = field,
            FromValue = fromValue,
            ToValue = toValue,
            Detail = detail,
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(ct);
    }

    public async Task<PagedResult<AuditLog>> SearchAsync(string? jobNo, string? user, AuditActionType? action,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _context.AuditLogs.AsNoTracking().OrderByDescending(a => a.Timestamp).AsQueryable();

        if (!string.IsNullOrWhiteSpace(jobNo))
            query = query.Where(a => a.JobNo != null && a.JobNo.Contains(jobNo));
        if (!string.IsNullOrWhiteSpace(user))
            query = query.Where(a => a.User == user);
        if (action.HasValue)
            query = query.Where(a => a.Action == action);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<AuditLog> { Items = items, TotalCount = total, Page = page, PageSize = pageSize };
    }

    public async Task<IReadOnlyList<string>> GetDistinctUsersAsync(CancellationToken ct = default) =>
        await _context.AuditLogs.AsNoTracking().Select(a => a.User).Distinct().OrderBy(u => u).ToListAsync(ct);
}
