using CTD_FINAL.Interfaces;
using CTD_FINAL.Constants;
using CTD_FINAL.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CTD_FINAL.Controllers;

[Authorize]
[RequirePermission(PermissionKeys.ReportsViewExport)]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService) => _reportService = reportService;

    private static readonly string[] ValidKeys =
    {
        "dailyJob", "ctdRegister", "pendingCtd", "containerMovement", "deliveryStatus", "customerRevenue", "billingSummary"
    };

    public IActionResult Index()
    {
        ViewData["Title"] = "Reports";
        ViewData["Breadcrumb"] = "CTD Suite / Reports";
        ViewData["ActiveNav"] = "reports";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Table(string key)
    {
        if (!ValidKeys.Contains(key)) return NotFound();
        var result = await _reportService.GetReportAsync(key);
        if (result is null) return NotFound();
        return PartialView("_ReportTable", result);
    }
}
