using CTD_FINAL.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CTD_FINAL.Infrastructure.Authorization;

/// <summary>
/// Enforces the seeded Role Permission Matrix (Users &amp; Roles screen) on a
/// controller/action, mirroring the prototype's applyRolePermissions()/
/// PERMISSION_MATRIX_DEFAULT-driven UI hiding — but as real server-side
/// authorization instead of client-side hide/show.
/// </summary>
public class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string moduleKey) : base(typeof(RequirePermissionFilter))
    {
        Arguments = new object[] { moduleKey };
    }
}

public class RequirePermissionFilter : IAsyncAuthorizationFilter
{
    private readonly string _moduleKey;
    private readonly IPermissionService _permissionService;

    public RequirePermissionFilter(string moduleKey, IPermissionService permissionService)
    {
        _moduleKey = moduleKey;
        _permissionService = permissionService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new ChallengeResult();
            return;
        }

        var roles = user.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value);
        foreach (var role in roles)
        {
            if (await _permissionService.IsAllowedAsync(role, _moduleKey, context.HttpContext.RequestAborted))
                return;
        }

        context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
    }
}
