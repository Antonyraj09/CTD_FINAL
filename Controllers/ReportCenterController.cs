using CTD_FINAL.Constants;
using CTD_FINAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CTD_FINAL.Controllers;

/// <summary>
/// "Report Center" — a single search box across every major record type (Job ISNE,
/// Delivery ISNE, CTD Job, Party, Sub-Agent, Transit Route, Documents). Deliberately not
/// gated behind one new blanket permission — a category only ever appears in results when
/// the signed-in user's role already has the same permission that gates that entity's own
/// screen, so this can never surface something the user couldn't already see by navigating
/// there directly, and no tenant needs to grant a brand-new permission just to get the
/// feature at all.
/// </summary>
[Authorize]
public class ReportCenterController : Controller
{
    private readonly IReportCenterService _reportCenterService;
    private readonly IPermissionService _permissionService;

    public ReportCenterController(IReportCenterService reportCenterService, IPermissionService permissionService)
    {
        _reportCenterService = reportCenterService;
        _permissionService = permissionService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        ViewData["Title"] = "Report Center";
        ViewData["Breadcrumb"] = "Eroyal Suite / Report Center";
        ViewData["ActiveNav"] = "report-center";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Search(string q, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return Json(new { categories = Array.Empty<object>() });

        var allowed = await AllowedCategoriesAsync(ct);
        var categories = await _reportCenterService.SearchAsync(q, allowed, ct);
        return Json(new { categories });
    }

    private async Task<HashSet<string>> AllowedCategoriesAsync(CancellationToken ct)
    {
        var roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();

        async Task<bool> CanAsync(string moduleKey)
        {
            foreach (var role in roles)
                if (await _permissionService.IsAllowedAsync(role, moduleKey, ct))
                    return true;
            return false;
        }

        var allowed = new HashSet<string>();
        if (await CanAsync(PermissionKeys.JobIsneManage)) allowed.Add("jobIsne");
        if (await CanAsync(PermissionKeys.DeliveryIsneManage)) allowed.Add("deliveryIsne");
        if (await CanAsync(PermissionKeys.JobCreateEdit))
        {
            allowed.Add("ctdJob");
            allowed.Add("document");
        }
        if (await CanAsync(PermissionKeys.MasterDataManage))
        {
            allowed.Add("party");
            allowed.Add("subAgent");
            allowed.Add("transitRoute");
        }
        return allowed;
    }
}
