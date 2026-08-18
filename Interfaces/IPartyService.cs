using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;

namespace CTD_FINAL.Interfaces;

public interface IPartyService
{
    Task<IReadOnlyList<Party>> SearchAsync(string? query, CancellationToken ct = default);
    Task<Party?> GetByIdAsync(int id, CancellationToken ct = default);

    /// <summary>Computed live from existing PartyCodes on every call (MAX matching the name's
    /// 2-letter prefix + 1) rather than a persisted counter — same reasoning as
    /// NumberSequenceService.NextIsneJobNumberAsync: deletes are real deletes here, so the
    /// next code should always reflect what's actually left.</summary>
    Task<string> PeekNextCodeAsync(string? name, CancellationToken ct = default);
    Task<Party> SaveAsync(Party party, List<PartyBranchDto> branches, string userName, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, string userName, CancellationToken ct = default);
}
