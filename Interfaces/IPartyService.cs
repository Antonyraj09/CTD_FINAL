using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;

namespace CTD_FINAL.Interfaces;

public interface IPartyService
{
    Task<IReadOnlyList<Party>> SearchAsync(string? query, CancellationToken ct = default);
    Task<Party?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Party> SaveAsync(Party party, List<PartyBranchDto> branches, string userName, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, string userName, CancellationToken ct = default);
}
