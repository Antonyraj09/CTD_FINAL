using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Constants;
using CTD_FINAL.Infrastructure.Authorization;
using CTD_FINAL.Models.PartyMaster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CTD_FINAL.Controllers;

[Authorize]
[RequirePermission(PermissionKeys.MasterDataManage)]
public class PartyController : Controller
{
    private readonly IPartyService _partyService;
    private readonly IAuditService _auditService;

    public PartyController(IPartyService partyService, IAuditService auditService)
    {
        _partyService = partyService;
        _auditService = auditService;
    }

    private string CurrentUserName => User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "System";

    public IActionResult Index()
    {
        ViewData["Title"] = "Party Master";
        ViewData["Breadcrumb"] = "CTD Suite / Entry-Edit / Party";
        ViewData["ActiveNav"] = "party";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Table(string? q)
    {
        var records = await _partyService.SearchAsync(q);
        return PartialView("_PartyTable", records);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        ViewData["Title"] = id.HasValue ? "Edit Party" : "New Party";
        ViewData["Breadcrumb"] = "CTD Suite / Entry-Edit / Party";
        ViewData["ActiveNav"] = "party";

        Party? party = id.HasValue ? await _partyService.GetByIdAsync(id.Value) : null;
        if (id.HasValue && party is null) return NotFound();

        return View(party);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromBody] PartySaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Json(new { success = false, message = "Party name is required." });

        if (!request.IsImporter && !request.IsTransporter && !request.IsAgent)
            return Json(new { success = false, message = "Select at least one role (Importer / Transporter / Agent)." });

        var branches = request.Branches.Where(b => !string.IsNullOrWhiteSpace(b.BranchName) && !string.IsNullOrWhiteSpace(b.City)).ToList();
        if (branches.Count == 0)
            return Json(new { success = false, message = "At least one branch with a name and city is required." });

        if (!branches.Any(b => b.IsPrimary))
            branches[0].IsPrimary = true;

        var party = new Party
        {
            Id = request.Id,
            Name = request.Name,
            TradeName = request.TradeName,
            Constitution = ParseConstitution(request.Constitution),
            Pan = NullIfEmpty(request.Pan),
            IecCode = NullIfEmpty(request.IecCode),
            CinNumber = NullIfEmpty(request.CinNumber),
            IsImporter = request.IsImporter,
            IsTransporter = request.IsTransporter,
            IsAgent = request.IsAgent,
            License = NullIfEmpty(request.License),
            LicenseValidUpto = request.LicenseValidUpto,
            Fleet = NullIfEmpty(request.Fleet),
            AeoStatus = ParseAeoStatus(request.AeoStatus),
            AeoCertificateNo = NullIfEmpty(request.AeoCertificateNo),
            BankName = NullIfEmpty(request.BankName),
            BankAccountNo = NullIfEmpty(request.BankAccountNo),
            BankIfsc = NullIfEmpty(request.BankIfsc),
            AdCode = NullIfEmpty(request.AdCode),
            Website = NullIfEmpty(request.Website),
            ContactPersonName = NullIfEmpty(request.ContactPersonName),
            ContactPersonDesignation = NullIfEmpty(request.ContactPersonDesignation),
            ContactPersonPhone = NullIfEmpty(request.ContactPersonPhone),
            ContactPersonEmail = NullIfEmpty(request.ContactPersonEmail),
            IsActive = request.IsActive,
            Remarks = NullIfEmpty(request.Remarks)
        };

        var branchDtos = branches.Select(b => new PartyBranchDto
        {
            BranchName = b.BranchName,
            IsPrimary = b.IsPrimary,
            IsActive = b.IsActive,
            AddressLine1 = b.AddressLine1,
            AddressLine2 = NullIfEmpty(b.AddressLine2),
            City = b.City,
            State = NullIfEmpty(b.State),
            PinCode = NullIfEmpty(b.PinCode),
            Country = string.IsNullOrWhiteSpace(b.Country) ? "India" : b.Country,
            Gstin = NullIfEmpty(b.Gstin),
            Phone = NullIfEmpty(b.Phone),
            Email = NullIfEmpty(b.Email),
            ContactPersonName = NullIfEmpty(b.ContactPersonName),
            CustomsRegistrationNo = NullIfEmpty(b.CustomsRegistrationNo)
        }).ToList();

        try
        {
            var saved = await _partyService.SaveAsync(party, branchDtos, CurrentUserName);
            return Json(new { success = true, id = saved.Id, message = $"{saved.Name} {(request.Id == 0 ? "added" : "updated")} successfully" });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException?.Message.Contains("IX_Parties") == true || ex.InnerException?.Message.Contains("IX_PartyBranches") == true)
        {
            return Json(new { success = false, message = "PAN, IEC, CIN, License and GSTIN must each be unique — one of these is already used by another record." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var party = await _partyService.GetByIdAsync(id);
        if (party is null) return Json(new { success = false, message = "Party not found." });

        try
        {
            var deleted = await _partyService.DeleteAsync(id, CurrentUserName);
            if (!deleted) return Json(new { success = false, message = "Party not found." });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return Json(new { success = false, message = $"{party.Name} is used by one or more CTD jobs and cannot be deleted." });
        }

        return Json(new { success = true, message = $"{party.Name} removed" });
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private static PartyConstitution ParseConstitution(string s) =>
        Enum.TryParse<PartyConstitution>(s, true, out var v) ? v : PartyConstitution.PrivateLimited;

    private static AeoStatus ParseAeoStatus(string s) =>
        Enum.TryParse<AeoStatus>(s, true, out var v) ? v : AeoStatus.None;
}
