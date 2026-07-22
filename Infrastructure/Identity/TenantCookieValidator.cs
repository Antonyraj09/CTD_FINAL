using CTD_FINAL.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace CTD_FINAL.Infrastructure.Identity;

/// <summary>
/// Wired as the application cookie's OnValidatePrincipal event (see Program.cs's
/// ConfigureApplicationCookie), replacing the default AddIdentity assignment
/// (SecurityStampValidator.ValidatePrincipalAsync, i.e. ValidateAsync&lt;ISecurityStampValidator&gt;).
///
/// OnValidatePrincipal fires inside UseAuthentication(), for every single request that
/// carries a valid auth cookie — not periodically. The default handler resolves
/// ISecurityStampValidator via DI to call it, and merely CONSTRUCTING that validator
/// (SecurityStampValidator&lt;ApplicationUser&gt;) pulls in SignInManager/UserManager/AppDbContext — before
/// TenantResolutionMiddleware (which runs later, after UseAuthentication()) ever gets a
/// chance to establish a tenant. Every authenticated request was hitting "No tenant database
/// connection has been established for this request." as a result, regardless of
/// SecurityStampValidatorOptions.ValidationInterval (that option only skips the stamp
/// comparison INSIDE an already-constructed validator; it never prevented the construction).
///
/// This wrapper resolves the tenant from the LicenseNumber claim FIRST — touching only
/// AdminDbContext, never AppDbContext — populates ITenantContextAccessor for this request,
/// and only then delegates to the default security-stamp check, which can now safely
/// construct AppDbContext-backed services. This subsumes what TenantResolutionMiddleware did
/// (same claim, same ITenantResolutionService call, same reject-and-sign-out-on-invalid-license
/// behavior) since it now runs unconditionally on every authenticated request just the same —
/// that middleware has been removed to avoid two competing implementations of the same check.
/// </summary>
public static class TenantCookieValidator
{
    public static async Task ValidateAsync(CookieValidatePrincipalContext context)
    {
        var principal = context.Principal;
        if (principal?.Identity?.IsAuthenticated != true)
            return;

        var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("TenantCookieValidator");
        var licenseNumber = principal.FindFirst("LicenseNumber")?.Value;
        if (string.IsNullOrEmpty(licenseNumber))
        {
            logger.LogWarning("Authenticated request with no LicenseNumber claim on {Path}; signing out.", context.HttpContext.Request.Path);
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
            return;
        }

        var tenantResolutionService = context.HttpContext.RequestServices.GetRequiredService<ITenantResolutionService>();
        var resolution = await tenantResolutionService.ResolveAsync(licenseNumber, context.HttpContext.RequestAborted);
        if (!resolution.LicenseValid || resolution.ConnectionString is null)
        {
            logger.LogWarning("License {LicenseNumber} is no longer valid ({Reason}); signing out.", licenseNumber, resolution.FailureReason);
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync();
            return;
        }

        var tenantContextAccessor = context.HttpContext.RequestServices.GetRequiredService<ITenantContextAccessor>();
        tenantContextAccessor.Set(resolution.ConnectionString, licenseNumber, resolution.CompanyId, resolution.CompanyName);

        // Now safe: constructing the registered ISecurityStampValidator needs SignInManager/
        // UserManager, which need AppDbContext, which needs the tenant set immediately above.
        // Must be the interface, not the concrete SecurityStampValidator<ApplicationUser> type
        // — AddIdentity registers it as services.TryAddScoped<ISecurityStampValidator,
        // SecurityStampValidator<TUser>>(), so only the interface is resolvable via DI.
        await SecurityStampValidator.ValidateAsync<ISecurityStampValidator>(context);
    }
}
