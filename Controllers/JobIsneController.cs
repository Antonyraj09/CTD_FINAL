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

        foreach (var c in request.Containers)
        {
            if (string.IsNullOrEmpty(c.ContainerNo)) continue;
            if (c.ContainerNo.Length > 15)
                return Json(new { success = false, message = $"Container Number '{c.ContainerNo}' cannot exceed 15 characters." });
            if (!System.Text.RegularExpressions.Regex.IsMatch(c.ContainerNo, "^[a-zA-Z0-9]*$"))
                return Json(new { success = false, message = $"Container Number '{c.ContainerNo}' must be alphanumeric only — no special characters." });
        }

        if (!Enum.TryParse<ContainerStatus>(request.ShipmentType, true, out var shipmentType))
            shipmentType = ContainerStatus.FCL;

        // Entry for Data Sheet fields are all optional — only format/length is
        // re-checked server-side, and only when a value was actually provided.
        if (!string.IsNullOrEmpty(request.ImporterCode) && !System.Text.RegularExpressions.Regex.IsMatch(request.ImporterCode, "^[A-Za-z]{2}[0-9]{4}$"))
            return Json(new { success = false, message = "Importer Code must be 2 letters followed by exactly 4 numeric digits." });

        if (!string.IsNullOrEmpty(request.InvoiceNumber) && request.InvoiceNumber.Length > 20)
            return Json(new { success = false, message = "Invoice Number cannot exceed 20 characters." });

        if (!string.IsNullOrEmpty(request.CertificateOfOrigin) && request.CertificateOfOrigin.Length > 30)
            return Json(new { success = false, message = "Certificate of Origin cannot exceed 30 characters." });

        if (!string.IsNullOrEmpty(request.InsuranceCompanyNameAddress) && request.InsuranceCompanyNameAddress.Length > 200)
            return Json(new { success = false, message = "Insurance Company Name & Address cannot exceed 200 characters." });

        if (request.SensitiveCifValue.HasValue && request.SensitiveCifValue.Value <= 0)
            return Json(new { success = false, message = "CIF Value must be a positive number." });

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
            ShipmentType = shipmentType,
            MiscDescription = request.MiscDescription,
            CargoDescription = request.CargoDescription,
            ImporterCode = request.ImporterCode,
            InvoiceNumber = request.InvoiceNumber,
            InvoiceDate = request.InvoiceDate,
            CertificateOfOrigin = request.CertificateOfOrigin,
            CertificateOfOriginDate = request.CertificateOfOriginDate,
            SensitiveCargo = request.SensitiveCargo,
            InsuranceCompanyNameAddress = request.SensitiveCargo ? request.InsuranceCompanyNameAddress : null,
            SensitiveCifValue = request.SensitiveCargo ? request.SensitiveCifValue : null,
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

        var containers = request.Containers.Select(c =>
        {
            Enum.TryParse<ContainerStatus>(c.ShipmentType, true, out var rowShipmentType);
            return new JobIsneContainer
            {
                ContainerNo = c.ContainerNo,
                ContainerSize = c.ContainerSize,
                ShipmentType = rowShipmentType,
                NoPackages = c.NoPackages,
                PackageType = c.PackageType,
                GrossWeight = c.GrossWeight,
                GrossWeightUnit = c.GrossWeightUnit,
                NetWeight = c.NetWeight,
                NetWeightUnit = c.NetWeightUnit,
                MarksSerial = c.MarksSerial,
                CustomsCode = c.CustomsCode
            };
        }).ToList();

        try
        {
            var saved = await _jobIsneService.SaveAsync(entity, containers, CurrentUserName);
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

    /// <summary>
    /// CTD Submission checklist — the "ANNEXURE - A" data sheet, field-for-field matching
    /// the reference format supplied for this feature: fetched values in red, static labels
    /// in black, downloaded as a Word document rather than opened for browser printing.
    /// Served as plain HTML with Word-compatible markup (the well-established technique
    /// Word itself supports for opening ".doc" files) rather than real OOXML — no new
    /// dependency, same convention as every other print view in this app. Fields JobIsne
    /// doesn't capture (Importer's PAN, Insurance Policy details, Anti-Dumping Duty,
    /// Vehicle/Chassis/Engine No.) are left as blank fill-in lines rather than guessed —
    /// same convention the paper form itself uses for optional fields.
    /// </summary>
    [HttpGet]
    [RequirePermission(PermissionKeys.JobIsneManage)]
    public async Task<IActionResult> CtdSubmission(int id)
    {
        var record = await _jobIsneService.GetByIdAsync(id);
        if (record is null) return NotFound();

        ViewBag.Agent = await LoadAgentAsync(record);

        var result = View(record);
        result.ContentType = "application/msword";
        Response.Headers["Content-Disposition"] = $"attachment; filename=\"CTD_Checklist_{record.JobNumber}.doc\"";
        return result;
    }

    /// <summary>
    /// "Customs Transit Declaration (Import) ICCD" — the official CHA-issued form layout,
    /// replicated field-for-field from the reference paper format. Opened for browser
    /// printing (window.print(), print-to-PDF via the browser's own dialog) same as every
    /// other print view in this app — no server-side PDF generation.
    /// </summary>
    [HttpGet]
    [RequirePermission(PermissionKeys.JobIsneManage)]
    public async Task<IActionResult> CtdDeclaration(int id)
    {
        var record = await _jobIsneService.GetByIdAsync(id);
        if (record is null) return NotFound();

        ViewBag.Agent = await LoadAgentAsync(record);

        return View(record);
    }

    private async Task<SubAgent?> LoadAgentAsync(JobIsne record) =>
        string.IsNullOrEmpty(record.SubAgentCode)
            ? null
            : (await _subAgents.FindAsync(s => s.SubAgentCode == record.SubAgentCode)).FirstOrDefault();

    /// <summary>
    /// "Undertaking Bond" — the legal bond an importer executes when cargo is NOT sensitive,
    /// pledging to pay the President of India the difference between the goods' market
    /// value and CIF value if they fail to reach Nepal. Only offered for non-sensitive cargo
    /// (SensitiveCargo == false) because the reference form's bond amount is specifically
    /// Market Value minus CIF Value — sensitive cargo prices off SensitiveCifValue instead,
    /// which this bond format doesn't account for, so direct access is blocked rather than
    /// producing a bond with a meaningless amount.
    /// </summary>
    [HttpGet]
    [RequirePermission(PermissionKeys.JobIsneManage)]
    public async Task<IActionResult> UndertakingBond(int id)
    {
        var record = await _jobIsneService.GetByIdAsync(id);
        if (record is null) return NotFound();
        if (record.SensitiveCargo) return BadRequest("Undertaking Bond is only applicable when Sensitive Cargo is set to No.");

        ViewBag.Agent = await LoadAgentAsync(record);
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
