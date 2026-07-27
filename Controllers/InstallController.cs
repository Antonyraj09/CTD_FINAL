using System.Security.Cryptography;
using System.Text;
using CTD_FINAL.Constants;
using CTD_FINAL.Data;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Models.Install;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Controllers;

/// <summary>
/// The Installation Wizard (Steps 1-4 of the spec): collects Client Information and
/// Database Configuration, then drives ProvisioningService to create the tenant database
/// and register it in ADMIN_CTD. Open to anonymous users only until the first company is
/// registered — after that, re-running it requires either the shared Setup:InstallKey (no
/// sign-in needed, e.g. for scripted/first-touch provisioning), or being signed in as an
/// Administrator under the designated "master" license (Setup:MasterLicenseNumber, defaults
/// to ERC00001 — the license every fresh deployment's first tenant is issued) — the license
/// the reseller/operator uses day-to-day to onboard new clients, without needing to carry the
/// shared key around separately. Every other license's Administrator gets neither path.
/// </summary>
[AllowAnonymous]
public class InstallController : Controller
{
    private readonly AdminDbContext _adminContext;
    private readonly IProvisioningService _provisioningService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InstallController> _logger;

    public InstallController(AdminDbContext adminContext, IProvisioningService provisioningService, IConfiguration configuration, ILogger<InstallController> logger)
    {
        _adminContext = adminContext;
        _provisioningService = provisioningService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? key)
    {
        var hasExistingCompany = await _adminContext.Companies.AnyAsync();
        if (hasExistingCompany && !IsSetupKeyValid(key) && !IsMasterLicenseAdmin())
            return View("Locked", !string.IsNullOrEmpty(key)); // model: true = a key WAS supplied and was wrong, false = none supplied yet

        return View(new InstallIndexViewModel { RequiresSetupKey = hasExistingCompany, SetupKey = key });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Provision([FromBody] InstallProvisionRequest request)
    {
        var hasExistingCompany = await _adminContext.Companies.AnyAsync();
        if (hasExistingCompany && !IsSetupKeyValid(request.SetupKey) && !IsMasterLicenseAdmin())
            return Json(new { success = false, message = "A valid setup key is required to run the installer again." });

        if (!ModelState.IsValid)
        {
            var firstError = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).FirstOrDefault();
            return Json(new { success = false, message = firstError ?? "Please correct the highlighted fields." });
        }

        if (!Enum.TryParse<LicenseType>(request.LicenseType, ignoreCase: true, out var licenseType))
            return Json(new { success = false, message = "Invalid license type." });

        var provisioningRequest = new ProvisioningRequest(
            request.CompanyName, request.CompanyCode, request.Address, request.Country, request.State, request.City,
            request.GstNumber, request.ContactPerson, request.Email, request.Phone, request.InstallationLocation,
            licenseType, request.DatabaseName, request.DatabaseUsername, request.DatabasePassword,
            request.AdminEmail, request.AdminFullName, request.AdminPassword,
            InstalledBy: User.Identity?.IsAuthenticated == true ? (User.Identity.Name ?? "Unknown") : "Install Wizard",
            MachineName: Environment.MachineName);

        var result = await _provisioningService.ProvisionAsync(provisioningRequest);
        if (!result.Success)
        {
            _logger.LogWarning("Installation failed for company {CompanyCode}: {Reason}", request.CompanyCode, result.FailureReason);
            return Json(new { success = false, message = result.FailureReason ?? "Installation failed." });
        }

        return Json(new
        {
            success = true,
            licenseNumber = result.LicenseNumber,
            companyCode = result.CompanyCode,
            message = $"Installation complete. License number {result.LicenseNumber} has been issued."
        });
    }

    /// <summary>True when the current request is signed in as an Administrator under the
    /// designated master license — the reseller/operator's own day-to-day login, allowed to
    /// re-run the wizard without the shared setup key.</summary>
    private bool IsMasterLicenseAdmin()
    {
        if (User.Identity?.IsAuthenticated != true || !User.IsInRole(RoleNames.Administrator)) return false;

        var masterLicense = _configuration["Setup:MasterLicenseNumber"];
        if (string.IsNullOrEmpty(masterLicense)) return false;

        var currentLicense = User.FindFirst("LicenseNumber")?.Value;
        return string.Equals(currentLicense, masterLicense, StringComparison.OrdinalIgnoreCase);
    }

    private bool IsSetupKeyValid(string? suppliedKey)
    {
        var configuredKey = _configuration["Setup:InstallKey"];
        if (string.IsNullOrEmpty(configuredKey) || string.IsNullOrEmpty(suppliedKey)) return false;

        var configuredBytes = Encoding.UTF8.GetBytes(configuredKey);
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey);
        return configuredBytes.Length == suppliedBytes.Length && CryptographicOperations.FixedTimeEquals(configuredBytes, suppliedBytes);
    }
}
