using CTD_FINAL.Constants;
using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Infrastructure.Authorization;
using CTD_FINAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Controllers;

/// <summary>
/// "Delivery — ISNE": records physical delivery of a Job ISNE's cargo. Opening the
/// module always lands on the List screen first (per spec) — New/Edit/View all live
/// on the same Entry screen, distinguished by whether an id is supplied and whether
/// it's opened in read-only (View) mode.
/// </summary>
[Authorize]
public class DeliveryIsneController : Controller
{
    private readonly IDeliveryIsneService _deliveryIsneService;
    private readonly IGenericRepository<JobIsne> _jobIsnes;
    private readonly IGenericRepository<Party> _parties;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeliveryIsneController(IDeliveryIsneService deliveryIsneService, IGenericRepository<JobIsne> jobIsnes,
        IGenericRepository<Party> parties, UserManager<ApplicationUser> userManager)
    {
        _deliveryIsneService = deliveryIsneService;
        _jobIsnes = jobIsnes;
        _parties = parties;
        _userManager = userManager;
    }

    private string CurrentUserName => User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "System";

    [HttpGet]
    [RequirePermission(PermissionKeys.DeliveryIsneManage)]
    public IActionResult Index()
    {
        ViewData["Title"] = "Delivery — ISNE";
        ViewData["Breadcrumb"] = "Eroyal Suite / Delivery";
        ViewData["ActiveNav"] = "delivery-isne";
        ViewData["ActiveModule"] = "delivery";
        return View();
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.DeliveryIsneManage)]
    public async Task<IActionResult> ListTable(string? serialNo, string? jobNo, DateTime? dateFrom, DateTime? dateTo,
        string? customer, string? transporter, string? quick, string sortKey = "deliveryDate", string sortDir = "desc", int page = 1)
    {
        var filter = new DeliveryIsneFilter
        {
            SerialNo = serialNo, JobNo = jobNo, DateFrom = dateFrom, DateTo = dateTo,
            Customer = customer, Transporter = transporter, Quick = quick, SortKey = sortKey, SortDir = sortDir
        };
        var result = await _deliveryIsneService.SearchAsync(filter, page, 10);
        ViewData["SortKey"] = sortKey;
        ViewData["SortDir"] = sortDir;
        return PartialView("_DeliveryTable", result);
    }

    /// <summary>New Delivery (id=null), Edit (id set), or View (id set, view=true).</summary>
    [HttpGet]
    [RequirePermission(PermissionKeys.DeliveryIsneManage)]
    public async Task<IActionResult> Entry(int? id, bool view = false)
    {
        ViewData["Title"] = id.HasValue ? "Delivery — ISNE" : "Delivery — ISNE (New)";
        ViewData["Breadcrumb"] = "Eroyal Suite / Delivery";
        ViewData["ActiveNav"] = "delivery-isne";
        ViewData["ActiveModule"] = "delivery";
        ViewData["ViewOnly"] = view;

        DeliveryIsne? record = id.HasValue ? await _deliveryIsneService.GetByIdAsync(id.Value) : null;
        if (id.HasValue && record is null) return NotFound();

        // Raw entity lists — the anonymous-typed client lookup blobs (JobLookup/TransporterLookup/
        // StaffLookup JSON) are projected from these directly in Entry.cshtml, same convention as
        // JobIsne/Index.cshtml's partyLookup/subAgentLookup cascade.
        ViewBag.Jobs = await _jobIsnes.Query().Include(j => j.Containers).OrderByDescending(j => j.JobNumber).ToListAsync();
        ViewBag.Transporters = await _parties.Query().Where(p => p.IsTransporter && p.IsActive).OrderBy(p => p.Name).ToListAsync();
        ViewBag.Staff = await _userManager.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();

        return View(record);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(PermissionKeys.DeliveryIsneManage)]
    public async Task<IActionResult> Save([FromBody] DeliveryIsneSaveRequest request)
    {
        if (request.JobIsneId <= 0)
            return Json(new { success = false, message = "Please select Job No." });
        if (request.DeliveryDate == default)
            return Json(new { success = false, message = "Delivery Date is required." });
        if (!request.Package.HasValue || request.Package.Value <= 0)
            return Json(new { success = false, message = "Package must be greater than zero." });
        if (!request.TransporterId.HasValue)
            return Json(new { success = false, message = "Transporter is required." });
        if (!request.StaffId.HasValue)
            return Json(new { success = false, message = "Staff is required." });

        var job = await _jobIsnes.Query().Include(j => j.Containers).FirstOrDefaultAsync(j => j.Id == request.JobIsneId);
        if (job is null)
            return Json(new { success = false, message = "Please select Job No." });
        if (job.Containers.Any() && string.IsNullOrWhiteSpace(request.ContainerNo))
            return Json(new { success = false, message = "Container is required for this Job." });

        var entity = new DeliveryIsne
        {
            Id = request.Id,
            DeliveryDate = request.DeliveryDate,
            PartYN = request.PartYN == "Y" ? "Y" : "N",
            JobIsneId = job.Id,
            JobNo = job.JobNumber,
            CustomerName = job.PartyName,
            ConsigneeName = request.ConsigneeName,
            TruckRailwayReckNo = request.TruckRailwayReckNo,
            Shed = request.Shed,
            KeyNo = request.KeyNo,
            Package = request.Package,
            Route = request.Route,
            TransporterId = request.TransporterId,
            TransporterCode = request.TransporterCode,
            TransporterName = request.TransporterName,
            BslNo = request.BslNo,
            StaffId = request.StaffId,
            StaffCode = request.StaffCode,
            StaffName = request.StaffName,
            ContainerNo = request.ContainerNo,
            ContainerSize = request.ContainerSize,
            Remarks = request.Remarks
        };

        try
        {
            var saved = await _deliveryIsneService.SaveAsync(entity, CurrentUserName);
            var message = request.Id == 0 ? "Delivery saved successfully." : "Delivery updated successfully.";
            return Json(new { success = true, id = saved.Id, serialNo = saved.SerialNo, message });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(PermissionKeys.DeliveryIsneManage)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _deliveryIsneService.DeleteAsync(id, CurrentUserName);
        return Json(new { success = deleted, message = deleted ? "Delivery deleted successfully." : "Delivery not found." });
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.DeliveryIsneManage)]
    public async Task<IActionResult> Print(int id)
    {
        var record = await _deliveryIsneService.GetByIdAsync(id);
        if (record is null) return NotFound();
        return View(record);
    }
}
