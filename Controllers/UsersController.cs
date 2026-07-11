using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Constants;
using CTD_FINAL.Helpers;
using CTD_FINAL.Infrastructure.Authorization;
using CTD_FINAL.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Controllers;

[Authorize]
[RequirePermission(PermissionKeys.UsersRolesManage)]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IPermissionService _permissionService;
    private readonly IAuditService _auditService;

    public UsersController(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
        IPermissionService permissionService, IAuditService auditService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _permissionService = permissionService;
        _auditService = auditService;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Users & Roles";
        ViewData["Breadcrumb"] = "CTD Suite / System";
        ViewData["ActiveNav"] = "users";

        var users = await _userManager.Users.OrderBy(u => u.FullName).ToListAsync();
        var rows = new List<(ApplicationUser User, string Role)>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            rows.Add((u, roles.FirstOrDefault() ?? ""));
        }
        ViewBag.Users = rows;
        ViewBag.Roles = RoleNames.All;
        ViewBag.PermissionModules = PermissionKeys.All;
        ViewBag.Matrix = await _permissionService.GetMatrixAsync();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Form(int? id)
    {
        UserFormViewModel model;
        if (id.HasValue)
        {
            var user = await _userManager.FindByIdAsync(id.Value.ToString());
            if (user is null) return NotFound();
            var roles = await _userManager.GetRolesAsync(user);
            model = new UserFormViewModel { Id = user.Id, FullName = user.FullName, Email = user.Email!, Role = roles.FirstOrDefault() ?? RoleNames.Viewer, IsActive = user.IsActive };
        }
        else
        {
            model = new UserFormViewModel { Role = RoleNames.Viewer };
        }
        ViewBag.Roles = RoleNames.All;
        return PartialView("_UserForm", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(UserFormViewModel model)
    {
        if (!ModelState.IsValid)
            return Json(new { success = false, message = "Please complete all required fields with valid values." });

        if (model.Id == 0)
        {
            var existing = await _userManager.FindByEmailAsync(model.Email);
            if (existing is not null)
                return Json(new { success = false, message = "A user with this email already exists." });

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                IsActive = model.IsActive,
                EmailConfirmed = true
            };
            var tempPassword = PasswordGenerator.Generate();
            var result = await _userManager.CreateAsync(user, tempPassword);
            if (!result.Succeeded)
                return Json(new { success = false, message = string.Join(" ", result.Errors.Select(e => e.Description)) });

            await _userManager.AddToRoleAsync(user, model.Role);
            await _auditService.LogAsync(AuditActionType.Created, User.Identity?.Name ?? "System", detail: $"User '{model.FullName}' created with role {model.Role}");

            return Json(new { success = true, message = $"{model.FullName} added as {model.Role}.", tempPassword });
        }
        else
        {
            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user is null) return Json(new { success = false, message = "User not found." });

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.IsActive = model.IsActive;
            await _userManager.UpdateAsync(user);

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Contains(model.Role))
            {
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, model.Role);
            }

            await _auditService.LogAsync(AuditActionType.Updated, User.Identity?.Name ?? "System", detail: $"User '{model.FullName}' updated (role: {model.Role}, active: {model.IsActive})");
            return Json(new { success = true, message = $"{model.FullName} updated." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return Json(new { success = false, message = "User not found." });

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        await _auditService.LogAsync(AuditActionType.Updated, User.Identity?.Name ?? "System", detail: $"User '{user.FullName}' {(user.IsActive ? "enabled" : "disabled")}");

        return Json(new { success = true, isActive = user.IsActive, message = $"{user.FullName} {(user.IsActive ? "enabled" : "disabled")}." });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user is null) return Json(new { success = false, message = "User not found." });

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var newPassword = PasswordGenerator.Generate();
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
            return Json(new { success = false, message = string.Join(" ", result.Errors.Select(e => e.Description)) });

        await _auditService.LogAsync(AuditActionType.Updated, User.Identity?.Name ?? "System", detail: $"Password reset for user '{user.FullName}'");
        return Json(new { success = true, message = $"Password reset for {user.FullName}.", newPassword });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePermissionMatrix()
    {
        var matrix = new Dictionary<string, Dictionary<string, bool>>();
        foreach (var role in RoleNames.All)
        {
            matrix[role] = new Dictionary<string, bool>();
            foreach (var (key, _) in PermissionKeys.All)
                matrix[role][key] = Request.Form[$"perm_{role}_{key}"] == "on";
        }

        // Guard against locking every admin out of user/role management by mistake.
        matrix[RoleNames.Administrator][PermissionKeys.UsersRolesManage] = true;

        await _permissionService.SaveMatrixAsync(matrix);
        await _auditService.LogAsync(AuditActionType.Updated, User.Identity?.Name ?? "System", detail: "Role permission matrix updated");

        return Json(new { success = true, message = "Permission matrix saved." });
    }
}
