using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Constants;
using CTD_FINAL.Infrastructure.Authorization;
using CTD_FINAL.Models.JobIsne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Controllers;

[Authorize]
public class JobIsneController : Controller
{
    private readonly IJobIsneService _jobIsneService;
    private readonly IGenericRepository<Party> _parties;
    private readonly IGenericRepository<SubAgent> _subAgents;
    private readonly IGenericRepository<TransitRoute> _transitRoutes;

    public JobIsneController(IJobIsneService jobIsneService, IGenericRepository<Party> parties,
        IGenericRepository<SubAgent> subAgents, IGenericRepository<TransitRoute> transitRoutes)
    {
        _jobIsneService = jobIsneService;
        _parties = parties;
        _subAgents = subAgents;
        _transitRoutes = transitRoutes;
    }

    private string CurrentUserName => User.FindFirst("FullName")?.Value ?? User.Identity?.Name ?? "System";

    [HttpGet]
    [RequirePermission(PermissionKeys.JobIsneManage)]
    public async Task<IActionResult> Index(int? id)
    {
        ViewData["Title"] = "Job — ISNE";
        ViewData["Breadcrumb"] = "CTD Suite / Jobs / ISNE";
        ViewData["ActiveNav"] = "job-isne";
        ViewData["ActiveModule"] = "jobs";

        JobIsne? record = id.HasValue ? await _jobIsneService.GetByIdAsync(id.Value) : null;
        ViewBag.NextJobNumber = record?.JobNumber ?? await _jobIsneService.PeekNextJobNumberAsync();

        var parties = await _parties.Query().Include(p => p.Branches).Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
        ViewBag.Parties = parties;
        ViewBag.SubAgents = (await _subAgents.GetAllAsync()).OrderBy(s => s.Name).ToList();
        ViewBag.TransitRoutes = (await _transitRoutes.GetAllAsync()).OrderBy(r => r.Name).ToList();

        return View(record);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(PermissionKeys.JobIsneManage)]
    public async Task<IActionResult> Save([FromBody] JobIsneSaveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PartyCode) || string.IsNullOrWhiteSpace(request.PartyName))
            return Json(new { success = false, message = "Party Code and Party Name are required." });

        if (!string.IsNullOrEmpty(request.CtdNumber))
        {
            if (request.CtdNumber.Length > 25)
                return Json(new { success = false, message = "CTD Number cannot exceed 25 characters." });
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.CtdNumber, "^[a-zA-Z0-9]*$"))
                return Json(new { success = false, message = "CTD Number must be alphanumeric only — no special characters." });
        }

        if (!string.IsNullOrEmpty(request.ContainerNo))
        {
            if (request.ContainerNo.Length > 15)
                return Json(new { success = false, message = "Container Number cannot exceed 15 characters." });
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.ContainerNo, "^[a-zA-Z0-9]*$"))
                return Json(new { success = false, message = "Container Number must be alphanumeric only — no special characters." });
        }

        if (!Enum.TryParse<ContainerStatus>(request.ContainerStatus, true, out var containerStatus))
            containerStatus = ContainerStatus.FCL;

        var entity = new JobIsne
        {
            Id = request.Id,
            JobDate = request.JobDate == default ? DateTime.UtcNow.Date : request.JobDate,
            PartyCode = request.PartyCode,
            PartyName = request.PartyName,
            Address = request.Address,
            SubAgentCode = request.SubAgentCode,
            SubAgentName = request.SubAgentName,
            CtdNumber = request.CtdNumber,
            CtdDate = request.CtdDate,
            VesselName = request.VesselName,
            VoyageNo = request.VoyageNo,
            TsVessel = request.TsVessel,
            TsVoyage = request.TsVoyage,
            CountryCgn = request.CountryCgn,
            CountryOrigin = request.CountryOrigin,
            RouteOfTransit = request.RouteOfTransit,
            RotNo = request.RotNo,
            RotDate = request.RotDate,
            InwardDate = request.InwardDate,
            LineNo = request.LineNo,
            MblNo = request.MblNo,
            MblDate = request.MblDate,
            HblNo = request.HblNo,
            HblDate = request.HblDate,
            IlNo = request.IlNo,
            IlDate = request.IlDate,
            LcNo = request.LcNo,
            LcDate = request.LcDate,
            AccountName = request.AccountName,
            BankName = request.BankName,
            RefNo = request.RefNo,
            RefDate = request.RefDate,
            SteamerAgent = request.SteamerAgent,
            ContainerAgent = request.ContainerAgent,
            VesselArrival = request.VesselArrival,
            CtdSentTo = request.CtdSentTo,
            GreenCtd = request.GreenCtd,
            DuePackingList = request.DuePackingList,
            DueInvoice = request.DueInvoice,
            DueOriginalBl = request.DueOriginalBl,
            DueInsuranceCert = request.DueInsuranceCert,
            DueLcCopy = request.DueLcCopy,
            DueLoa = request.DueLoa,
            DueOrigin = request.DueOrigin,
            DueProformaInvoice = request.DueProformaInvoice,
            MarksSerial = request.MarksSerial,
            ContainerNo = request.ContainerNo,
            ContainerStatus = containerStatus,
            ContainerSize = request.ContainerSize,
            NoPackages = request.NoPackages,
            CustomsCode = request.CustomsCode,
            MiscDescription = request.MiscDescription,
            Unit = request.Unit,
            CargoDescription = request.CargoDescription,
            Currency = request.Currency,
            ExchangeRate = request.ExchangeRate,
            FobValue = request.FobValue,
            Freight = request.Freight,
            CifFc = request.CifFc,
            CifFcReference = request.CifFcReference,
            InsuranceFc = request.InsuranceFc,
            InsuranceValue = request.InsuranceValue,
            InsuranceExRate = request.InsuranceExRate,
            InsuranceRate = request.InsuranceRate,
            InsuranceValueInr = request.InsuranceValueInr,
            CifInr = request.CifInr,
            MarketRate = request.MarketRate,
            MarketValueInr = request.MarketValueInr,
            GrossWeight = request.GrossWeight,
            NetWeight = request.NetWeight,
            LcAmount = request.LcAmount,
            ShipmentExpiry = request.ShipmentExpiry,
            PartialShipment = request.PartialShipment,
            DutyAmount = request.DutyAmount
        };

        try
        {
            var saved = await _jobIsneService.SaveAsync(entity, CurrentUserName);
            return Json(new { success = true, id = saved.Id, jobNumber = saved.JobNumber, message = $"ISNE Job {saved.JobNumber} saved successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission(PermissionKeys.JobIsneManage)]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _jobIsneService.DeleteAsync(id, CurrentUserName);
        return Json(new { success = deleted, message = deleted ? "ISNE Job removed" : "Job ISNE not found." });
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.JobIsneManage)]
    public async Task<IActionResult> Print(int id)
    {
        var record = await _jobIsneService.GetByIdAsync(id);
        if (record is null) return NotFound();
        return View(record);
    }

    [RequirePermission(PermissionKeys.TrackingView)]
    public IActionResult Tracking()
    {
        ViewData["Title"] = "CTD Tracking";
        ViewData["Breadcrumb"] = "CTD Suite / Operations";
        ViewData["ActiveNav"] = "tracking";
        ViewData["ActiveModule"] = "jobs";
        return View();
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.TrackingView)]
    public async Task<IActionResult> TrackingTable(string? jobNo, string? ctdNo, string? partyName, string? container,
        DateTime? dateFrom, DateTime? dateTo, string? status, string? quick, string sortKey = "jobDate", string sortDir = "desc", int page = 1)
    {
        var filter = new JobIsneTrackingFilter
        {
            JobNo = jobNo, CtdNo = ctdNo, PartyName = partyName, Container = container,
            DateFrom = dateFrom, DateTo = dateTo, Status = status, Quick = quick, SortKey = sortKey, SortDir = sortDir
        };
        var result = await _jobIsneService.SearchAsync(filter, page, 8);
        ViewData["SortKey"] = sortKey;
        ViewData["SortDir"] = sortDir;
        return PartialView("_TrackingTable", result);
    }
}
