using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Constants;
using CTD_FINAL.Infrastructure.Authorization;
using CTD_FINAL.Models.Audit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CTD_FINAL.Controllers;

[Authorize]
[RequirePermission(PermissionKeys.AuditHistoryView)]
public class AuditController : Controller
{
    private readonly IAuditService _auditService;

    public AuditController(IAuditService auditService) => _auditService = auditService;

    public async Task<IActionResult> Index(string? jobNo, string? user, [FromQuery(Name = "actionType")] AuditActionType? actionType, int page = 1)
    {
        ViewData["Title"] = "Audit History";
        ViewData["Breadcrumb"] = "CTD Suite / System";
        ViewData["ActiveNav"] = "audit";

        var result = await _auditService.SearchAsync(jobNo, user, actionType, page, 50);
        var model = new AuditIndexViewModel
        {
            Result = result,
            Users = await _auditService.GetDistinctUsersAsync(),
            JobNo = jobNo,
            User = user,
            Action = actionType
        };
        return View(model);
    }
}
