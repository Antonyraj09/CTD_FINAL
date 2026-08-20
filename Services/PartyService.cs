using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Helpers;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

public class PartyService : IPartyService
{
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;

    public PartyService(AppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<PagedResult<PartyListItem>> SearchAsync(string? query, int page, int pageSize, CancellationToken ct = default)
    {
        var q = _context.Parties.AsNoTracking().OrderBy(p => p.Name).AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            q = q.Where(p => p.Name.Contains(term)
                || (p.PartyCode != null && p.PartyCode.Contains(term))
                || (p.TradeName != null && p.TradeName.Contains(term))
                || (p.Pan != null && p.Pan.Contains(term))
                || (p.IecCode != null && p.IecCode.Contains(term))
                || p.Branches.Any(b => b.Gstin != null && b.Gstin.Contains(term))
                || p.Branches.Any(b => b.City.Contains(term)));
        }

        // Project down to just the columns the list row renders — and only the primary
        // branch's name/city/GSTIN, not every field of every branch — instead of pulling
        // each party's full branch/registration detail via Include(), which grows heavy
        // once parties carry several state-wise GSTIN branches each.
        var projected = q.Select(p => new PartyListItem
        {
            Id = p.Id,
            PartyCode = p.PartyCode,
            Name = p.Name,
            TradeName = p.TradeName,
            IsImporter = p.IsImporter,
            IsTransporter = p.IsTransporter,
            IsAgent = p.IsAgent,
            Pan = p.Pan,
            IecCode = p.IecCode,
            IsActive = p.IsActive,
            Branches = p.Branches.Select(b => new PartyListBranchItem
            {
                IsPrimary = b.IsPrimary,
                BranchName = b.BranchName,
                City = b.City,
                Gstin = b.Gstin
            }).ToList()
        });

        return await projected.ToPagedResultAsync(page, pageSize, ct);
    }

    public async Task<Party?> GetByIdAsync(int id, CancellationToken ct = default) =>
        await _context.Parties.AsNoTracking().Include(p => p.Branches).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Party>> SearchByCodePrefixAsync(string prefix, CancellationToken ct = default) =>
        await _context.Parties.AsNoTracking()
            .Where(p => p.PartyCode != null && p.PartyCode.StartsWith(prefix))
            .OrderBy(p => p.PartyCode)
            .Take(10)
            .ToListAsync(ct);

    public async Task<Party> SaveAsync(Party party, List<PartyBranchDto> branches, string userName, CancellationToken ct = default)
    {
        bool isNew = party.Id == 0;
        Party entity;

        if (isNew)
        {
            entity = party;
            _context.Parties.Add(entity);
        }
        else
        {
            entity = await _context.Parties.Include(p => p.Branches).FirstAsync(p => p.Id == party.Id, ct);
            var branchesToRemove = entity.Branches.ToList();
            _context.Entry(entity).CurrentValues.SetValues(party);
            entity.UpdatedAt = DateTime.UtcNow;
            _context.PartyBranches.RemoveRange(branchesToRemove);
            entity.Branches.Clear();
        }

        foreach (var b in branches)
        {
            entity.Branches.Add(new PartyBranch
            {
                BranchName = b.BranchName,
                IsPrimary = b.IsPrimary,
                IsActive = b.IsActive,
                AddressLine1 = b.AddressLine1,
                AddressLine2 = b.AddressLine2,
                City = b.City,
                State = b.State,
                PinCode = b.PinCode,
                Country = string.IsNullOrWhiteSpace(b.Country) ? "Nepal" : b.Country,
                Gstin = b.Gstin,
                Phone = b.Phone,
                Email = b.Email,
                ContactPersonName = b.ContactPersonName,
                CustomsRegistrationNo = b.CustomsRegistrationNo
            });
        }

        await _context.SaveChangesAsync(ct);
        await _auditService.LogAsync(isNew ? AuditActionType.Created : AuditActionType.Updated,
            userName, detail: $"Party '{entity.Name}' {(isNew ? "created" : "updated")} ({entity.Branches.Count} branch(es))");

        return entity;
    }

    public async Task<bool> DeleteAsync(int id, string userName, CancellationToken ct = default)
    {
        var party = await _context.Parties.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (party is null) return false;

        _context.Parties.Remove(party);
        await _context.SaveChangesAsync(ct);
        await _auditService.LogAsync(AuditActionType.Deleted, userName, detail: $"Party '{party.Name}' removed");
        return true;
    }
}
