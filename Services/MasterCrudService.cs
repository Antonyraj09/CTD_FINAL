using System.Reflection;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

/// <summary>
/// One generic service backs all 7 master entities, exactly the way the
/// prototype's MASTER_CONFIG + openMasterForm() drove a single reusable modal
/// CRUD flow instead of one screen/service per entity.
/// </summary>
public class MasterCrudService<T> : IMasterCrudService<T> where T : class
{
    private readonly AppDbContext _context;
    private static readonly PropertyInfo[] StringProps =
        typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(string))
            .ToArray();

    public MasterCrudService(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<T>> SearchAsync(string? query, CancellationToken ct = default)
    {
        var items = await _context.Set<T>().AsNoTracking().ToListAsync(ct);
        if (string.IsNullOrWhiteSpace(query)) return items;

        var q = query.Trim();
        return items.Where(item => StringProps.Any(p =>
            (p.GetValue(item) as string)?.Contains(q, StringComparison.OrdinalIgnoreCase) == true)).ToList();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default) => await _context.Set<T>().FindAsync(new object?[] { id }, ct);

    public async Task<T> CreateAsync(T entity, CancellationToken ct = default)
    {
        _context.Set<T>().Add(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken ct = default)
    {
        _context.Set<T>().Update(entity);
        await _context.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var entity = await _context.Set<T>().FindAsync(new object?[] { id }, ct);
        if (entity is null) return false;
        _context.Set<T>().Remove(entity);
        await _context.SaveChangesAsync(ct);
        return true;
    }
}
