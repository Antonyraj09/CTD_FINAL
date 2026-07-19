using System.Security.Claims;
using CTD_FINAL.Entities;
using CTD_FINAL.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CTD_FINAL.Infrastructure.Identity;

/// <summary>
/// Adds FullName as a claim so views/layout can render it without an extra DB round-trip,
/// and embeds the LicenseNumber the user signed in under — this is what lets every
/// subsequent request resolve which tenant database to use (TenantResolutionMiddleware
/// reads this same claim), without the app ever holding a fixed connection string.
/// AccountController.Login populates ITenantContextAccessor with the resolved tenant
/// BEFORE calling SignInManager, so it's already set by the time this factory runs.
/// </summary>
public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public AppUserClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> options, ITenantContextAccessor tenantContextAccessor) : base(userManager, roleManager, options)
    {
        _tenantContextAccessor = tenantContextAccessor;
    }

    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var principal = await base.CreateAsync(user);
        var identity = (ClaimsIdentity)principal.Identity!;
        identity.AddClaim(new Claim("FullName", user.FullName));
        if (!string.IsNullOrEmpty(_tenantContextAccessor.LicenseNumber))
            identity.AddClaim(new Claim("LicenseNumber", _tenantContextAccessor.LicenseNumber));
        return principal;
    }
}
