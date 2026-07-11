using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Constants;
using CTD_FINAL.Infrastructure.Authorization;
using CTD_FINAL.Models.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CTD_FINAL.Controllers;

[Authorize]
public class JobsController : Controller
{
    private readonly IJobService _jobService;
    private readonly IDocumentService _documentService;
    private readonly IPermissionService _permissionService;
    private readonly INumberSequenceService _numberSequenceService;
    private readonly IGenericRepository<Party> _parties;
    private readonly IGenericRepository<BorderPoint> _borderPoints;
    private readonly IGenericRepository<Commodity> _commodities;
    private readonly IGenericRepository<CustomsHouse> _customsHouses;
    private readonly IGenericRepository<TransitRoute> _transitRoutes;

    public JobsController(IJobService jobService, IDocumentService documentService, IPermissionService permissionService,
        INumberSequenceService numberSequenceService,
        IGenericRepository<Party> parties,
        IGenericRepository<BorderPoint> borderPoints, IGenericRepository<Commodity> commodities,
        IGenericRepository<CustomsHouse> customsHouses, IGenericRepository<TransitRoute> transitRoutes)
    {
        _jobService = jobService;
        _documentService = documentService;
        _permissionService = permissionService;
        _numberSequenceService = numberSequenceService;
        _parties = parties;
        _borderPoints = borderPoints;
        _commodities = commodities;
        _customsHouses = customsHouses;
        _transitRoutes = transitRoutes;
    }

    private string CurrentUserName => User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "System";

    [RequirePermission(PermissionKeys.JobCreateEdit)]
    public async Task<IActionResult> Wizard(int? id)
    {
        ViewData["ActiveNav"] = "wizard";
        ViewData["ActiveModule"] = "jobs";

        CtdJob? job = id.HasValue ? await _jobService.GetByIdAsync(id.Value) : null;
        if (id.HasValue && job is null) return NotFound();

        ViewData["Title"] = job is null ? "New CTD Job" : $"Edit CTD Job — {job.JobNo}";
        ViewData["Breadcrumb"] = "CTD Suite / Operations";

        var model = new JobWizardViewModel
        {
            Job = job,
            Importers = await _parties.FindAsync(p => p.IsImporter),
            Agents = await _parties.FindAsync(p => p.IsAgent),
            Transporters = await _parties.FindAsync(p => p.IsTransporter),
            BorderPoints = await _borderPoints.GetAllAsync(),
            Commodities = await _commodities.GetAllAsync(),
            CustomsHouses = await _customsHouses.GetAllAsync(),
            TransitRoutes = await _transitRoutes.GetAllAsync(),
            DefaultChecklist = await _jobService.GetDefaultChecklistAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(PermissionKeys.JobCreateEdit)]
    public async Task<IActionResult> Save([FromBody] JobSaveRequest request)
    {
        if (request.CloseJob && !await HasPermissionAsync(PermissionKeys.JobClose))
            return Json(new { success = false, message = "You do not have permission to close jobs." });

        if (request.Id == 0 && (!request.ImporterId.HasValue || !request.TransporterId.HasValue))
            return Json(new { success = false, message = "Importer and Transporter are required." });

        var job = new CtdJob
        {
            Id = request.Id,
            JobDate = request.JobDate,
            ShipmentType = request.ShipmentType == "multiple" ? ShipmentType.Multiple : ShipmentType.Single,
            ImporterId = request.ImporterId,
            AgentId = request.AgentId,
            TransporterId = request.TransporterId,
            OriginCountry = request.OriginCountry,
            PortArrival = request.PortArrival,
            BorderPointId = request.BorderPointId,
            Remarks = request.Remarks,

            InvoiceNo = request.InvoiceNo,
            InvoiceDate = request.InvoiceDate,
            Currency = request.Currency,
            InvoiceValue = request.InvoiceValue,
            CommodityId = request.CommodityId,
            HsCode = request.HsCode,
            GrossWt = request.GrossWt,
            NetWt = request.NetWt,
            Packages = request.Packages,

            CtdType = request.CtdType == "Manual" ? CtdType.Manual : CtdType.EDI,
            CtdNumber = request.CtdNumber,
            CtdDate = request.CtdDate,
            CustomsHouseId = request.CustomsHouseId,
            TransitRouteId = request.TransitRouteId,
            ExpDeliveryDate = request.ExpDeliveryDate,

            ArrivalDate = request.ArrivalDate,
            DeliveryDate = request.DeliveryDate,
            DeliveryStatus = ParseDeliveryStatus(request.DeliveryStatus),
            ServiceCharge = request.ServiceCharge,
            TransportCharge = request.TransportCharge,
            OtherCharge = request.OtherCharge,
            TaxPercent = request.TaxPercent,
            BillingStatus = ParseBillingStatus(request.BillingStatus)
        };

        var containers = request.Containers
            .Where(c => !string.IsNullOrWhiteSpace(c.ContainerNo))
            .Select(c => new JobContainerDto { ContainerNo = c.ContainerNo, Size = c.Size, Seal = c.Seal, Weight = c.Weight })
            .ToList();
        var checklist = request.Checklist.Select(c => new JobChecklistItemDto { Name = c.Name, Done = c.Done }).ToList();

        if (request.Id == 0 && containers.Count == 0)
            return Json(new { success = false, message = "At least one container with a container number is required." });

        var saved = await _jobService.SaveAsync(job, containers, checklist, request.CloseJob, CurrentUserName);

        return Json(new
        {
            success = true,
            id = saved.Id,
            jobNo = saved.JobNo,
            status = saved.Status.ToString(),
            total = saved.Total,
            message = request.CloseJob ? $"{saved.JobNo} has been finalized and closed" : $"{saved.JobNo} saved as {saved.Status}"
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(PermissionKeys.JobCreateEdit)]
    public async Task<IActionResult> GenerateDocument(int jobId, string docType)
    {
        var job = await _jobService.MarkDocumentGeneratedAsync(jobId, docType);
        if (job is null) return Json(new { success = false, message = "Job not found. Save the job first." });

        await _documentService.RegisterAsync(job.JobNo, job.Id, docType, "System (Auto-Generated)", systemGenerated: true);
        if (docType == "Forwarding Note")
            await _documentService.RegisterAsync(job.JobNo, job.Id, "Rail Form", "System (Auto-Generated)", systemGenerated: true);

        return Json(new { success = true, message = $"{docType} generated for {job.JobNo}" });
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.JobCreateEdit)]
    public async Task<IActionResult> DocumentPreview(int jobId, string docType)
    {
        var job = await _jobService.GetByIdAsync(jobId);
        if (job is null) return NotFound();
        ViewData["DocType"] = docType;
        return docType switch
        {
            "CTD Document" => PartialView("_CtdDocument", job),
            "Checklist" => PartialView("_ChecklistDocument", job),
            "Forwarding Note" => PartialView("_ForwardingDocument", job),
            _ => NotFound()
        };
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.JobCreateEdit)]
    public async Task<IActionResult> PrintSheet(int id)
    {
        var job = await _jobService.GetByIdAsync(id);
        if (job is null) return NotFound();
        return View(job);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(PermissionKeys.InvoiceGenerate)]
    public async Task<IActionResult> GenerateInvoice(int jobId)
    {
        var job = await _jobService.MarkInvoiceGeneratedAsync(jobId, CurrentUserName);
        if (job is null) return Json(new { success = false, message = "Job not found." });

        await _documentService.RegisterAsync(job.JobNo, job.Id, "Invoice", "System (Auto-Generated)", systemGenerated: true);
        return Json(new { success = true, message = $"Invoice created for {job.JobNo}" });
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.InvoiceGenerate)]
    public async Task<IActionResult> InvoicePreview(int id)
    {
        var job = await _jobService.GetByIdAsync(id);
        if (job is null) return NotFound();
        ViewData["InvoiceNo"] = await _numberSequenceService.NextInvoiceNumberAsync();
        return PartialView("_Invoice", job);
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.InvoiceGenerate)]
    public async Task<IActionResult> InvoicePrint(int id, string? invNo)
    {
        var job = await _jobService.GetByIdAsync(id);
        if (job is null) return NotFound();
        ViewData["InvoiceNo"] = invNo ?? await _numberSequenceService.NextInvoiceNumberAsync();
        return View(job);
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.TrackingView)]
    public async Task<IActionResult> ViewModal(int id)
    {
        var job = await _jobService.GetByIdAsync(id);
        if (job is null) return NotFound();
        return PartialView("_JobViewModal", job);
    }

    [RequirePermission(PermissionKeys.TrackingView)]
    public async Task<IActionResult> Tracking()
    {
        ViewData["Title"] = "CTD Tracking";
        ViewData["Breadcrumb"] = "CTD Suite / Operations";
        ViewData["ActiveNav"] = "tracking";
        ViewData["ActiveModule"] = "jobs";
        ViewBag.Importers = await _parties.FindAsync(p => p.IsImporter);
        return View();
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.TrackingView)]
    public async Task<IActionResult> TrackingTable(string? jobNo, string? ctdNo, int? importerId, string? container,
        DateTime? dateFrom, DateTime? dateTo, string? status, string? quick, string sortKey = "jobDate", string sortDir = "desc", int page = 1)
    {
        var filter = new TrackingFilter
        {
            JobNo = jobNo, CtdNo = ctdNo, ImporterId = importerId, Container = container,
            DateFrom = dateFrom, DateTo = dateTo, Status = status, Quick = quick, SortKey = sortKey, SortDir = sortDir
        };
        var result = await _jobService.SearchAsync(filter, page, 8);
        ViewData["SortKey"] = sortKey;
        ViewData["SortDir"] = sortDir;
        return PartialView("_TrackingTable", result);
    }

    private async Task<bool> HasPermissionAsync(string key)
    {
        var roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);
        foreach (var role in roles)
            if (await _permissionService.IsAllowedAsync(role, key)) return true;
        return false;
    }

    private static DeliveryStatusType ParseDeliveryStatus(string s) => s switch
    {
        "Arrived at Border" => DeliveryStatusType.ArrivedAtBorder,
        "Customs Cleared Nepal" => DeliveryStatusType.CustomsClearedNepal,
        "Delivered" => DeliveryStatusType.Delivered,
        "Delayed" => DeliveryStatusType.Delayed,
        _ => DeliveryStatusType.InTransit
    };

    private static BillingStatus ParseBillingStatus(string s) => s switch
    {
        "Partial" => BillingStatus.Partial,
        "Paid" => BillingStatus.Paid,
        _ => BillingStatus.Unpaid
    };
}
