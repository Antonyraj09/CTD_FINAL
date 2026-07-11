namespace CTD_FINAL.Interfaces;

/// <summary>
/// Generic CRUD service shared by the 7 master-data entities (Importer, Agent,
/// Transporter, Commodity, TransitRoute, BorderPoint, CustomsHouse) — mirrors
/// the prototype's single MASTER_CONFIG-driven modal CRUD behaviour instead of
/// duplicating near-identical service classes per entity.
/// </summary>
public interface IMasterCrudService<T> where T : class
{
    Task<IReadOnlyList<T>> SearchAsync(string? query, CancellationToken ct = default);
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<T> CreateAsync(T entity, CancellationToken ct = default);
    Task<T> UpdateAsync(T entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}
