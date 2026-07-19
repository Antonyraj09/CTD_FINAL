using CTD_FINAL.Entities;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CTD_FINAL.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILicenseService _licenseService;
    private readonly ITenantResolutionService _tenantResolutionService;
    private readonly ITenantContextAccessor _tenantContextAccessor;
    private readonly ILogger<AccountController> _logger;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager,
        ILicenseService licenseService, ITenantResolutionService tenantResolutionService, ITenantContextAccessor tenantContextAccessor,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _licenseService = licenseService;
        _tenantResolutionService = tenantResolutionService;
        _tenantContextAccessor = tenantContextAccessor;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToLocal(returnUrl);

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Steps 1-4 of the dynamic connection flow: License Number -> ADMIN_CTD -> read
        // database details -> decrypt connection. Both checks fold into one generic error
        // message so a bad license number can't be distinguished from a bad username/
        // password (avoids leaking which license numbers exist).
        var machineIdentifier = _licenseService.ComputeCurrentMachineIdentifier();
        var licenseResult = await _licenseService.ValidateAsync(model.LicenseNumber, machineIdentifier);
        if (!licenseResult.IsValid || licenseResult.License is null)
        {
            _logger.LogWarning("Login rejected: license {LicenseNumber} invalid ({Reason}).", model.LicenseNumber, licenseResult.FailureReason);
            ModelState.AddModelError(string.Empty, "Invalid license number, username or password.");
            return View(model);
        }

        var tenant = await _tenantResolutionService.ResolveAsync(model.LicenseNumber);
        if (!tenant.LicenseValid || tenant.ConnectionString is null)
        {
            _logger.LogWarning("Login rejected: license {LicenseNumber} could not resolve a tenant database ({Reason}).", model.LicenseNumber, tenant.FailureReason);
            ModelState.AddModelError(string.Empty, "Invalid license number, username or password.");
            return View(model);
        }

        // Step 5: connect to the client database — set BEFORE UserManager/SignInManager
        // touch AppDbContext for the first time this request, since its DI registration
        // reads this same accessor to build its connection.
        _tenantContextAccessor.Set(tenant.ConnectionString, model.LicenseNumber, tenant.CompanyId, tenant.CompanyName);

        // Step 6: authenticate the user from the client database, using ASP.NET Core
        // Identity's UserManager/SignInManager exactly as before — PBKDF2 password hashing,
        // never a reversible encryption, unchanged from the single-tenant implementation.
        var user = await _userManager.FindByEmailAsync(model.UserNameOrEmail)
                   ?? await _userManager.FindByNameAsync(model.UserNameOrEmail);

        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid license number, username or password.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            await _licenseService.RecordSuccessfulActivationAsync(licenseResult.License, machineIdentifier);
            _logger.LogInformation("User {Email} logged in under license {LicenseNumber}.", user.Email, model.LicenseNumber);
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account has been locked out due to multiple failed login attempts. Please try again later.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        // Placeholder flow: no outbound email provider is configured for this
        // environment, so we acknowledge the request without sending mail.
        if (!ModelState.IsValid) return View(model);

        _logger.LogInformation("Password reset requested for {Email} (email delivery not configured).", model.Email);
        return View("ForgotPasswordConfirmation");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl!);
        return RedirectToAction("Index", "Dashboard");
    }
}
