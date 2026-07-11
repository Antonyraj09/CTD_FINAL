using CTD_FINAL.Interfaces;
using CTD_FINAL.Constants;
using CTD_FINAL.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IGenericRepository<CTD_FINAL.Entities.Party> _partyRepository;

    public DashboardController(IDashboardService dashboardService, IGenericRepository<CTD_FINAL.Entities.Party> partyRepository)
    {
        _dashboardService = dashboardService;
        _partyRepository = partyRepository;
    }

    [RequirePermission(PermissionKeys.DashboardView)]
    public IActionResult Index()
    {
        ViewData["Title"] = "Dashboard";
        ViewData["Breadcrumb"] = "CTD Suite / Overview";
        ViewData["ActiveNav"] = "dashboard";
        return View();
    }

    [RequirePermission(PermissionKeys.CustomerDashboardView)]
    public async Task<IActionResult> Customer(int? importerId)
    {
        ViewData["Title"] = "Customer Dashboard";
        ViewData["Breadcrumb"] = "CTD Suite / Overview";
        ViewData["ActiveNav"] = "customerDashboard";

        var importers = await _partyRepository.FindAsync(p => p.IsImporter);
        ViewBag.Importers = importers.OrderBy(i => i.Name).ToList();
        ViewBag.SelectedImporterId = importerId ?? importers.FirstOrDefault()?.Id ?? 0;

        return View();
    }

    // ---- JSON endpoints consumed by wwwroot/js/dashboard.js ----

    [HttpGet]
    [RequirePermission(PermissionKeys.DashboardView)]
    public async Task<IActionResult> Kpis() => Json(await _dashboardService.GetKpisAsync());

    [HttpGet]
    [RequirePermission(PermissionKeys.DashboardView)]
    public async Task<IActionResult> MonthlyAggregate() => Json(await _dashboardService.GetMonthlyAggregateAsync());

    [HttpGet]
    [RequirePermission(PermissionKeys.DashboardView)]
    public async Task<IActionResult> StatusDistribution() => Json(await _dashboardService.GetStatusDistributionAsync());

    [HttpGet]
    [RequirePermission(PermissionKeys.DashboardView)]
    public async Task<IActionResult> BorderPointVolume() => Json(await _dashboardService.GetBorderPointVolumeAsync());

    [HttpGet]
    [RequirePermission(PermissionKeys.DashboardView)]
    public async Task<IActionResult> Alerts() => Json(await _dashboardService.GetAlertsAsync());

    [HttpGet]
    [RequirePermission(PermissionKeys.DashboardView)]
    public async Task<IActionResult> RecentJobs() => Json(await _dashboardService.GetRecentJobsAsync());

    [HttpGet]
    [RequirePermission(PermissionKeys.CustomerDashboardView)]
    public async Task<IActionResult> CustomerKpis(int importerId) => Json(await _dashboardService.GetCustomerKpisAsync(importerId));

    [HttpGet]
    [RequirePermission(PermissionKeys.CustomerDashboardView)]
    public async Task<IActionResult> CustomerShipments(int importerId) => Json(await _dashboardService.GetCustomerShipmentsAsync(importerId));

    [HttpGet]
    [RequirePermission(PermissionKeys.CustomerDashboardView)]
    public async Task<IActionResult> CustomerBilling(int importerId) => Json(await _dashboardService.GetCustomerBillingAsync(importerId));

    [HttpGet]
    [RequirePermission(PermissionKeys.CustomerDashboardView)]
    public async Task<IActionResult> CustomerJobOptions(int importerId)
    {
        var shipments = await _dashboardService.GetCustomerShipmentsAsync(importerId);
        return Json(shipments.Select(s => new { s.Id, label = $"{s.JobNo} — {s.Status}" }));
    }

    [HttpGet]
    [RequirePermission(PermissionKeys.CustomerDashboardView)]
    public async Task<IActionResult> ShipmentTimeline(int jobId) => Json(await _dashboardService.GetShipmentTimelineAsync(jobId));
}
