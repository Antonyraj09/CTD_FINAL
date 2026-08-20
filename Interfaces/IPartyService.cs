using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;

namespace CTD_FINAL.Interfaces;

public interface IPartyService
{
    Task<PagedResult<PartyListItem>> SearchAsync(string? query, int page, int pageSize, CancellationToken ct = default);
    Task<Party?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Party Code autocomplete — parties whose code starts with the given prefix,
    /// so the user can see which codes already exist in that series while typing a new one,
    /// or pick an existing party to jump straight to editing it.</summary>
    Task<IReadOnlyList<Party>> SearchByCodePrefixAsync(string prefix, CancellationToken ct = default);
    Task<Party> SaveAsync(Party party, List<PartyBranchDto> branches, string userName, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, string userName, CancellationToken ct = default);
}
