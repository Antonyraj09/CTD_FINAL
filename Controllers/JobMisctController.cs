using CTD_FINAL.Entities;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Constants;
using CTD_FINAL.Infrastructure.Authorization;
using CTD_FINAL.Models.JobMisct;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Controllers;

[Authorize]
public class JobMisctController : Controller
{
    private readonly IJobMisctService _jobMisctService;
    private readonly IGenericRepository<Party> _parties;
    private readonly IGenericRepository<SubAgent> _subAgents;
    private readonly IGenericRepository<CustomsHouse> _customsHouses;
    private readonly IGenericRepository<BorderPoint> _borderPoints;
    private readonly IGenericRepository<Commodity> _commodities;

    public JobMisctController(IJobMisctService jobMisctService, IGenericRepository<Party> parties,
        IGenericRepository<SubAgent> subAgents, IGenericRepository<CustomsHouse> customsHouses,
        IGenericRepository<BorderPoint> borderPoints, IGenericRepository<Commodity> commodities)
    {
        _jobMisctService = jobMisctService;
        _parties = parties;
        _subAgents = subAgents;
        _customsHouses = customsHouses;
        _borderPoints = borderPoints;
        _commodities = commodities;
    }

    private string CurrentUserName => User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "System";

    /// <summary>List screen — every MISCT job created so far, with a New button linking to
    /// Entry. Same List-first-then-Entry convention as Delivery ISNE.</summary>
    [HttpGet]
    [RequirePermission(PermissionKeys.MisctManage)]
    public IActionResult Index()
    {
        ViewData["Title"] = "Job — MISCT";
        ViewData["Breadcrumb"] = "Eroyal Suite / Jobs / MISCT";
        ViewData["ActiveNav"] = "job-misct";
        ViewData["ActiveModule"] = "jobs";
        return View();
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.MisctManage)]
    public async Task<IActionResult> Table(string? q, int page = 1)
    {
        var result = await _jobMisctService.SearchAsync(q, page, 25);
        return PartialView("_MisctTable", result);
    }

    /// <summary>New Job (id=null) or Edit (id set).</summary>
    [HttpGet]
    [RequirePermission(PermissionKeys.MisctManage)]
    public async Task<IActionResult> Entry(int? id)
    {
        ViewData["Title"] = id.HasValue ? "Job — MISCT" : "Job — MISCT (New)";
        ViewData["Breadcrumb"] = "Eroyal Suite / Jobs / MISCT";
        ViewData["ActiveNav"] = "job-misct";
        ViewData["ActiveModule"] = "jobs";

        MisctJob? record = id.HasValue ? await _jobMisctService.GetByIdAsync(id.Value) : null;
        if (id.HasValue && record is null) return NotFound();
        ViewBag.NextJobNumber = record?.JobNo ?? await _jobMisctService.PeekNextJobNumberAsync();

        var parties = await _parties.Query().Include(p => p.Branches).Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
        ViewBag.Parties = parties;
        ViewBag.SubAgents = (await _subAgents.GetAllAsync()).OrderBy(s => s.Name).ToList();
        ViewBag.CustomsHouses = (await _customsHouses.GetAllAsync()).OrderBy(c => c.Name).ToList();
        ViewBag.BorderPoints = (await _borderPoints.GetAllAsync()).OrderBy(b => b.Name).ToList();
        ViewBag.Commodities = (await _commodities.GetAllAsync()).OrderBy(c => c.HsCode).ToList();

        return View(record);
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.MisctManage)]
    public async Task<IActionResult> SuggestJobNo(string? prefix, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(prefix)) return Json(new { items = Array.Empty<object>() });

        var matches = await _jobMisctService.SearchByJobNoPrefixAsync(prefix.Trim(), ct);
        var items = matches.Select(j => new { id = j.Id, jobNo = j.JobNo, partyName = j.PartyName });
        return Json(new { items });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(PermissionKeys.MisctManage)]
    public async Task<IActionResult> Save([FromBody] JobMisctSaveRequest request)
    {
        if (request.JobDate == default)
            return Json(new { success = false, message = "Job Date is required." });

        if (string.IsNullOrWhiteSpace(request.PartyCode))
            return Json(new { success = false, message = "Party Code is required." });

        var party = await _parties.Query().FirstOrDefaultAsync(p => p.PartyCode == request.PartyCode);
        if (party is null)
            return Json(new { success = false, message = $"Party Code '{request.PartyCode}' does not exist. Please select a valid Party." });

        if (!string.IsNullOrWhiteSpace(request.SubAgentCode))
        {
            var subAgentExists = await _subAgents.Query().AnyAsync(s => s.SubAgentCode == request.SubAgentCode);
            if (!subAgentExists)
                return Json(new { success = false, message = $"Sub Agent Code '{request.SubAgentCode}' does not exist." });
        }

        if (request.CustomsStationExitId.HasValue && !await _customsHouses.Query().AnyAsync(c => c.Id == request.CustomsStationExitId.Value))
            return Json(new { success = false, message = "Selected Customs Stn of Exit is invalid." });

        if (request.PortOfEntryNepalId.HasValue && !await _borderPoints.Query().AnyAsync(b => b.Id == request.PortOfEntryNepalId.Value))
            return Json(new { success = false, message = "Selected Port of Entry in Nepal is invalid." });

        if (request.Container20Qty < 0 || request.Container40Qty < 0 || request.LclQty < 0)
            return Json(new { success = false, message = "Container size quantities cannot be negative." });

        if (request.NoOfPackage < 0)
            return Json(new { success = false, message = "No. of Package cannot be negative." });

        if (request.GrossWeight.HasValue && request.GrossWeight.Value < 0)
            return Json(new { success = false, message = "Gross Weight cannot be negative." });

        var containerNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var sealNos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in request.Containers)
        {
            if (!string.IsNullOrEmpty(c.ContainerNo))
            {
                if (c.ContainerNo.Length > 15)
                    return Json(new { success = false, message = $"Container Number '{c.ContainerNo}' cannot exceed 15 characters." });
                if (!System.Text.RegularExpressions.Regex.IsMatch(c.ContainerNo, "^[a-zA-Z0-9]*$"))
                    return Json(new { success = false, message = $"Container Number '{c.ContainerNo}' must be alphanumeric only — no special characters." });
                if (!containerNos.Add(c.ContainerNo))
                    return Json(new { success = false, message = $"Container number '{c.ContainerNo}' already exists in this Job." });
            }
            if (!string.IsNullOrEmpty(c.SealNo) && !sealNos.Add(c.SealNo))
                return Json(new { success = false, message = $"Seal number '{c.SealNo}' already exists in this Job." });
            if (c.Weight.HasValue && c.Weight.Value < 0)
                return Json(new { success = false, message = "Container Weight cannot be negative." });
            if (c.CifValue.HasValue && c.CifValue.Value < 0)
                return Json(new { success = false, message = "Container CIF Value cannot be negative." });
        }

        var customsHouse = request.CustomsStationExitId.HasValue
            ? await _customsHouses.Query().FirstOrDefaultAsync(c => c.Id == request.CustomsStationExitId.Value)
            : null;
        var borderPoint = request.PortOfEntryNepalId.HasValue
            ? await _borderPoints.Query().FirstOrDefaultAsync(b => b.Id == request.PortOfEntryNepalId.Value)
            : null;

        var entity = new MisctJob
        {
            Id = request.Id,
            JobDate = request.JobDate,
            PartyCode = request.PartyCode,
            PartyName = request.PartyName,
            Address = request.Address,
            SubAgentCode = request.SubAgentCode,
            SubAgentName = request.SubAgentName,
            VesselName = request.VesselName,
            VoyageNo = request.VoyageNo,
            CountryCgn = request.CountryCgn,
            RotNo = request.RotNo,
            RotDate = request.RotDate,
            LineNo = request.LineNo,
            MblNo = request.MblNo,
            MblDate = request.MblDate,
            CustomsStationExitId = request.CustomsStationExitId,
            CustomsStationExitName = customsHouse?.Name,
            PortOfEntryNepalId = request.PortOfEntryNepalId,
            PortOfEntryNepalName = borderPoint?.Name,
            PortOfEntryIndia = request.PortOfEntryIndia,
            BondNo = request.BondNo,
            Container20Qty = request.Container20Qty,
            Container40Qty = request.Container40Qty,
            LclQty = request.LclQty,
            CustomCode = request.CustomCode,
            NoOfPackage = request.NoOfPackage,
            Unit = request.Unit,
            Description = request.Description,
            GrossWeight = request.GrossWeight,
            InvoiceNo = request.InvoiceNo,
            InvoiceDate = request.InvoiceDate,
            CarrierName = request.CarrierName,
            CarrierAddress = request.CarrierAddress,
            CarrierGstin = request.CarrierGstin,
            Quantity = request.Quantity,
            QuantityUnit = request.QuantityUnit
        };

        var containers = request.Containers.Select(c => new MisctJobContainer
        {
            ContainerNo = c.ContainerNo,
            SealNo = c.SealNo,
            ContainerSize = c.ContainerSize,
            Weight = c.Weight,
            CifValue = c.CifValue
        }).ToList();

        try
        {
            var saved = await _jobMisctService.SaveAsync(entity, containers, CurrentUserName);
            return Json(new { success = true, id = saved.Id, jobNo = saved.JobNo, message = $"MISCT Job {saved.JobNo} saved successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return Json(new { success = false, message = "Unable to save MISCT Job — one or more fields are too long or otherwise violate a database constraint. Please try again." });
        }
    }

    /// <summary>Forwarding Note for General and Dangerous Merchandise — same field layout
    /// across the three terminal operators (CONCOR/Pristine/HTPL) this app's clients use,
    /// only the letterhead differs, so one shared view is parameterized by provider instead
    /// of tripling the markup.</summary>
    [HttpGet]
    [RequirePermission(PermissionKeys.MisctManage)]
    public async Task<IActionResult> ForwardingNote(int id, string provider = "concor")
    {
        var record = await _jobMisctService.GetByIdAsync(id);
        if (record is null) return NotFound();

        var providerKey = (provider ?? "concor").ToLowerInvariant();
        switch (providerKey)
        {
            case "pristine":
                ViewBag.ProviderName = "PRISTINE MEGA LOGISTICS PARK PVT. LTD.";
                ViewBag.ProviderNote = "(FOR PRISTINE ONLY)";
                ViewBag.ProviderService = "Pristine";
                break;
            case "htpl":
                ViewBag.ProviderName = "HIND TERMINALS PRIVATE LIMITED (HTPL)";
                ViewBag.ProviderNote = "(FOR HTPL USE ONLY)";
                ViewBag.ProviderService = "HTPL";
                break;
            default:
                providerKey = "concor";
                ViewBag.ProviderName = "CONTAINER CORPORATION OF INDIA LIMITED (CONCOR)";
                ViewBag.ProviderNote = "(FOR CONCOR USE ONLY)";
                ViewBag.ProviderService = "CONCOR";
                break;
        }
        ViewBag.ProviderKey = providerKey;
        ViewBag.ImporterPan = await LoadImporterPanAsync(record.PartyCode);
        return View(record);
    }

    /// <summary>"Declaration of transshipment" (Sea regulation 4) — filed by the authorised
    /// carrier declaring the goods' transit route from Indian port to Nepal border.</summary>
    [HttpGet]
    [RequirePermission(PermissionKeys.MisctManage)]
    public async Task<IActionResult> DeclarationOfTransshipment(int id)
    {
        var record = await _jobMisctService.GetByIdAsync(id);
        if (record is null) return NotFound();
        ViewBag.ImporterPan = await LoadImporterPanAsync(record.PartyCode);
        return View(record);
    }

    /// <summary>Transhipment Permit addressed to the Commissioner of Customs at the Indian
    /// port of entry, requesting permission to move the goods onward to Nepal.</summary>
    [HttpGet]
    [RequirePermission(PermissionKeys.MisctManage)]
    public async Task<IActionResult> TransshipmentPermit(int id)
    {
        var record = await _jobMisctService.GetByIdAsync(id);
        if (record is null) return NotFound();
        ViewBag.ImporterPan = await LoadImporterPanAsync(record.PartyCode);
        return View(record);
    }

    private async Task<string?> LoadImporterPanAsync(string partyCode) =>
        string.IsNullOrEmpty(partyCode)
            ? null
            : (await _parties.Query().FirstOrDefaultAsync(p => p.PartyCode == partyCode))?.Pan;

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(PermissionKeys.MisctManage)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _jobMisctService.DeleteAsync(id, CurrentUserName);
            return Json(new { success = deleted, message = deleted ? "MISCT Job removed" : "Job MISCT not found." });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return Json(new { success = false, message = "This Job is referenced by other records and cannot be deleted." });
        }
    }
}
