using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Constants;
using CTD_FINAL.Infrastructure.Authorization;
using CTD_FINAL.Models.Alerts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CTD_FINAL.Controllers;

[Authorize]
[RequirePermission(PermissionKeys.AlertRulesManage)]
public class AlertsController : Controller
{
    private readonly IAlertService _alertService;
    private readonly IAuditService _auditService;

    public AlertsController(IAlertService alertService, IAuditService auditService)
    {
        _alertService = alertService;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Email / SMS Alerts";
        ViewData["Breadcrumb"] = "CTD Suite / System";
        ViewData["ActiveNav"] = "alerts";

        ViewBag.Rules = await _alertService.GetRulesAsync();
        ViewBag.Logs = await _alertService.GetRecentLogAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRule(AlertRuleFormViewModel model)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.Name))
            return Json(new { success = false, message = "Please name this alert rule." });

        var channel = model.Channel switch
        {
            "SMS" => AlertChannel.Sms,
            "Email + SMS" => AlertChannel.EmailAndSms,
            _ => AlertChannel.Email
        };

        var rule = await _alertService.CreateRuleAsync(new AlertRule
        {
            Name = model.Name,
            Channel = channel,
            Trigger = string.IsNullOrWhiteSpace(model.Trigger) ? "Manual trigger" : model.Trigger,
            Audience = string.IsNullOrWhiteSpace(model.Audience) ? "All stakeholders" : model.Audience,
            Active = true
        });

        await _auditService.LogAsync(AuditActionType.Created, User.Identity?.Name ?? "System", detail: $"Alert rule '{rule.Name}' created");
        return Json(new { success = true, message = rule.Name });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRule(int id, bool active)
    {
        var rule = await _alertService.ToggleRuleAsync(id, active);
        if (rule is null) return Json(new { success = false, message = "Rule not found." });

        await _auditService.LogAsync(AuditActionType.Updated, User.Identity?.Name ?? "System", detail: $"Alert rule '{rule.Name}' {(active ? "enabled" : "disabled")}");
        return Json(new { success = true, message = $"{rule.Name} {(active ? "enabled" : "disabled")}" });
    }
}
