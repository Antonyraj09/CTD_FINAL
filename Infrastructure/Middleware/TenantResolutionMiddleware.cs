using CTD_FINAL.Interfaces;
using Microsoft.AspNetCore.Authentication;

namespace CTD_FINAL.Infrastructure.Middleware;

/// <summary>
/// Runs after UseAuthentication(), before UseAuthorization() — reads the LicenseNumber
/// claim off the current user's auth cookie (embedded by AppUserClaimsPrincipalFactory at
/// sign-in time) and populates the request-scoped ITenantContextAccessor before any
/// controller/filter/service touches AppDbContext. If the license has since become
/// invalid (suspended, expired, revoked, or its mapped database deactivated) the session
/// is signed out here rather than letting a stale cookie keep working against a database
/// it should no longer reach.
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolutionService tenantResolutionService, ITenantContextAccessor tenantContextAccessor)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var licenseNumber = context.User.FindFirst("LicenseNumber")?.Value;
        if (string.IsNullOrEmpty(licenseNumber))
        {
            // An authenticated principal with no LicenseNumber claim shouldn't happen once
            // every sign-in goes through the license-aware login flow — fail safe rather
            // than let a request through with no tenant resolved.
            _logger.LogWarning("Authenticated request with no LicenseNumber claim on {Path}; signing out.", context.Request.Path);
            await context.SignOutAsync();
            context.Response.Redirect("/Account/Login");
            return;
        }

        var resolution = await tenantResolutionService.ResolveAsync(licenseNumber, context.RequestAborted);
        if (!resolution.LicenseValid || resolution.ConnectionString is null)
        {
            _logger.LogWarning("License {LicenseNumber} is no longer valid ({Reason}); signing out.", licenseNumber, resolution.FailureReason);
            await context.SignOutAsync();
            context.Response.Redirect("/Account/Login");
            return;
        }

        tenantContextAccessor.Set(resolution.ConnectionString, licenseNumber, resolution.CompanyId, resolution.CompanyName);
        await _next(context);
    }
}

public static class TenantResolutionMiddlewareExtensions
{
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app) =>
        app.UseMiddleware<TenantResolutionMiddleware>();
}
