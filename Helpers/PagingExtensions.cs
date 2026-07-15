using CTD_FINAL.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Helpers;

public static class PagingExtensions
{
    /// <summary>
    /// Paginates in memory instead of translating Skip()/Take() to SQL Server's
    /// OFFSET/FETCH NEXT syntax, which only exists from SQL Server 2012 onward —
    /// this app needs to run against SQL Server 2008. Pulls the full filtered/sorted
    /// result set into the app before slicing, which is fine at this app's scale but
    /// means it isn't suitable for a table expected to grow into the millions of rows.
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int page, int pageSize, CancellationToken ct = default)
    {
        var all = await query.ToListAsync(ct);
        var items = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new PagedResult<T> { Items = items, TotalCount = all.Count, Page = page, PageSize = pageSize };
    }
}
