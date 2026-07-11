using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Constants;
using CTD_FINAL.Infrastructure.Authorization;
using CTD_FINAL.Infrastructure.Masters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CTD_FINAL.Controllers;

/// <summary>
/// One generic controller for all 7 master-data tabs, mirroring the
/// prototype's single MASTER_CONFIG-driven modal CRUD screen instead of a
/// controller per entity.
/// </summary>
[Authorize]
[RequirePermission(PermissionKeys.MasterDataManage)]
public class MastersController : Controller
{
    private readonly IServiceProvider _services;
    private readonly IAuditService _auditService;

    public MastersController(IServiceProvider services, IAuditService auditService)
    {
        _services = services;
        _auditService = auditService;
    }

    public IActionResult Index(string tab = "party")
    {
        var cfg = MasterRegistry.Get(tab);
        ViewData["Title"] = "Master Data — Setup";
        ViewData["Breadcrumb"] = "CTD Suite / Entry-Edit / Master";
        ViewData["ActiveNav"] = "masters";
        ViewData["ActiveTab"] = cfg.Key;
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Table(string tab, string? q)
    {
        var cfg = MasterRegistry.Get(tab);
        dynamic service = _services.GetRequiredService(typeof(IMasterCrudService<>).MakeGenericType(cfg.EntityType));
        var records = await service.SearchAsync(q, CancellationToken.None);

        ViewData["ActiveTab"] = cfg.Key;
        return PartialView("_MasterTable", (cfg, (IEnumerable<object>)records));
    }

    [HttpGet]
    public async Task<IActionResult> Form(string tab, int? id)
    {
        var cfg = MasterRegistry.Get(tab);
        object? record = null;
        if (id.HasValue)
        {
            dynamic service = _services.GetRequiredService(typeof(IMasterCrudService<>).MakeGenericType(cfg.EntityType));
            record = await service.GetByIdAsync(id.Value, CancellationToken.None);
        }
        return PartialView("_MasterForm", (cfg, record));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(string tab, int? id)
    {
        var cfg = MasterRegistry.Get(tab);
        var (valid, missingLabel) = MasterRegistry.Validate(Request.Form, cfg.Fields);
        if (!valid)
            return Json(new { success = false, message = $"{missingLabel} is required." });

        dynamic service = _services.GetRequiredService(typeof(IMasterCrudService<>).MakeGenericType(cfg.EntityType));
        object entity;
        bool isNew = !id.HasValue;

        if (isNew)
        {
            entity = Activator.CreateInstance(cfg.EntityType)!;
            MasterRegistry.BindFromForm(entity, Request.Form, cfg.Fields);
            dynamic newEntity = entity;
            entity = await service.CreateAsync(newEntity, CancellationToken.None);
        }
        else
        {
            entity = await service.GetByIdAsync(id!.Value, CancellationToken.None);
            if (entity is null) return Json(new { success = false, message = "Record not found." });
            MasterRegistry.BindFromForm(entity, Request.Form, cfg.Fields);
            dynamic existingEntity = entity;
            entity = await service.UpdateAsync(existingEntity, CancellationToken.None);
        }

        var name = MasterRegistry.GetString(entity, "Name");
        await _auditService.LogAsync(isNew ? AuditActionType.Created : AuditActionType.Updated,
            User.Identity?.Name ?? "System", detail: $"{cfg.EntityLabel} '{name}' {(isNew ? "created" : "updated")}");

        return Json(new { success = true, message = $"{name} {(isNew ? "added to" : "updated in")} {cfg.Title}" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string tab, int id)
    {
        var cfg = MasterRegistry.Get(tab);
        dynamic service = _services.GetRequiredService(typeof(IMasterCrudService<>).MakeGenericType(cfg.EntityType));
        var record = await service.GetByIdAsync(id, CancellationToken.None);
        if (record is null) return Json(new { success = false, message = "Record not found." });

        var name = MasterRegistry.GetString(record, "Name");
        try
        {
            bool deleted = await service.DeleteAsync(id, CancellationToken.None);
            if (!deleted) return Json(new { success = false, message = "Could not delete record." });
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            return Json(new { success = false, message = $"{name} is used by one or more CTD jobs and cannot be deleted." });
        }

        await _auditService.LogAsync(AuditActionType.Deleted, User.Identity?.Name ?? "System",
            detail: $"{cfg.EntityLabel} '{name}' removed from {cfg.Title}");

        return Json(new { success = true, message = $"{name} removed" });
    }
}
