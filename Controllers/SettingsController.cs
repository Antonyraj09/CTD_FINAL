using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Constants;
using CTD_FINAL.Models.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CTD_FINAL.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly ISettingsService _settingsService;
    private readonly IAuditService _auditService;

    public SettingsController(ISettingsService settingsService, IAuditService auditService)
    {
        _settingsService = settingsService;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Settings";
        ViewData["Breadcrumb"] = "CTD Suite / System";
        ViewData["ActiveNav"] = "settings";

        var settings = await _settingsService.GetAsync();
        var model = new SettingsFormViewModel
        {
            JobNumberPrefix = settings.JobNumberPrefix,
            InvoicePrefix = settings.InvoicePrefix,
            DocumentPrefix = settings.DocumentPrefix,
            CompanyName = settings.CompanyName,
            CompanyAddress = settings.CompanyAddress,
            CompanyGstin = settings.CompanyGstin,
            ChaLicenseNo = settings.ChaLicenseNo
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleNames.Administrator)]
    public async Task<IActionResult> Save(SettingsFormViewModel model)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = "Please complete all required fields." });

        await _settingsService.SaveAsync(new AppSettingsEntity
        {
            JobNumberPrefix = model.JobNumberPrefix,
            InvoicePrefix = model.InvoicePrefix,
            DocumentPrefix = model.DocumentPrefix,
            CompanyName = model.CompanyName,
            CompanyAddress = model.CompanyAddress,
            CompanyGstin = model.CompanyGstin,
            ChaLicenseNo = model.ChaLicenseNo
        });

        await _auditService.LogAsync(AuditActionType.Updated, User.Identity?.Name ?? "System", detail: "System settings updated");
        return Json(new { success = true, message = "Settings saved successfully." });
    }
}
