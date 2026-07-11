using CTD_FINAL.Entities;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace CTD_FINAL.Services;

public class PermissionService : IPermissionService
{
    private const string CacheKey = "RolePermissionMatrix";
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public PermissionService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<bool> IsAllowedAsync(string role, string moduleKey, CancellationToken ct = default)
    {
        var matrix = await GetMatrixAsync(ct);
        return matrix.TryGetValue(role, out var modules) && modules.TryGetValue(moduleKey, out var allowed) && allowed;
    }

    public async Task<Dictionary<string, Dictionary<string, bool>>> GetMatrixAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey, out Dictionary<string, Dictionary<string, bool>>? cached) && cached is not null)
            return cached;

        var rows = await _context.RolePermissions.AsNoTracking().ToListAsync(ct);
        var matrix = rows
            .GroupBy(r => r.Role)
            .ToDictionary(g => g.Key, g => g.ToDictionary(r => r.ModuleKey, r => r.Allowed));

        _cache.Set(CacheKey, matrix, TimeSpan.FromMinutes(10));
        return matrix;
    }

    public async Task SaveMatrixAsync(Dictionary<string, Dictionary<string, bool>> matrix, CancellationToken ct = default)
    {
        var existing = await _context.RolePermissions.ToListAsync(ct);
        var existingLookup = existing.ToDictionary(r => (r.Role, r.ModuleKey));

        foreach (var (role, modules) in matrix)
        {
            foreach (var (moduleKey, allowed) in modules)
            {
                if (existingLookup.TryGetValue((role, moduleKey), out var row))
                {
                    row.Allowed = allowed;
                }
                else
                {
                    _context.RolePermissions.Add(new RolePermission { Role = role, ModuleKey = moduleKey, Allowed = allowed });
                }
            }
        }

        await _context.SaveChangesAsync(ct);
        _cache.Remove(CacheKey);
    }
}
