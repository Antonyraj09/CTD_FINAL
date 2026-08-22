using CTD_FINAL.Entities;

namespace CTD_FINAL.Interfaces;

public interface IJobMisctService
{
    Task<MisctJob?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<string> PeekNextJobNumberAsync(CancellationToken ct = default);

    /// <summary>Backs the "Load Existing Job" typeahead on the entry screen — jobs whose
    /// JobNo starts with the given prefix, same convention as PartyService.SearchByCodePrefixAsync.</summary>
    Task<IReadOnlyList<MisctJob>> SearchByJobNoPrefixAsync(string prefix, CancellationToken ct = default);

    Task<MisctJob> SaveAsync(MisctJob record, List<MisctJobContainer> containers, string userName, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, string userName, CancellationToken ct = default);
}
