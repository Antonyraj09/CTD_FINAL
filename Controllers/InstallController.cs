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
    public async Task<IActionResult> Index(string? key, int? resumeId)
    {
        var hasExistingCompany = await _adminContext.Companies.AnyAsync();
        if (hasExistingCompany && !IsSetupKeyValid(key) && !IsMasterLicenseAdmin())
            return View("Locked", !string.IsNullOrEmpty(key)); // model: true = a key WAS supplied and was wrong, false = none supplied yet

        InstallPrefill? prefill = null;
        if (resumeId.HasValue)
        {
            // Only a genuinely incomplete attempt (no Company ever created) can be resumed —
            // a succeeded one has nothing left to continue, and re-running it would just
            // create a second, duplicate company.
            var history = await _adminContext.InstallationHistories
                .FirstOrDefaultAsync(h => h.Id == resumeId.Value && !h.CompanyId.HasValue);
            if (history is not null)
            {
                prefill = new InstallPrefill
                {
                    CompanyName = history.CompanyName,
                    CompanyCode = history.CompanyCode,
                    Address = history.Address,
                    Country = history.Country,
                    State = history.State,
                    City = history.City,
                    GstNumber = history.GstNumber,
                    ContactPerson = history.ContactPerson,
                    Email = history.Email,
                    Phone = history.Phone,
                    InstallationLocation = history.InstallationLocation,
                    LicenseType = history.LicenseType,
                    DatabaseName = history.DatabaseName,
                    DatabaseUsername = history.DatabaseUsername,
                    AdminFullName = history.AdminFullName,
                    AdminEmail = history.AdminEmail
                };
            }
        }

        return View(new InstallIndexViewModel { RequiresSetupKey = hasExistingCompany, SetupKey = key, Prefill = prefill });
    }

    /// <summary>Read-only view of every company/license/database provisioned so far, plus any
    /// provisioning attempt that failed before a Company row was even created (e.g. a database/
    /// login/schema step that errored out) — same access gate as re-running the wizard itself.
    /// Lets the master-license Administrator verify an install actually completed, drill into a
    /// completed one's (read-only) details, or resume an incomplete one, without needing direct
    /// database access. One merged, chronologically-sorted list rather than two separate tables
    /// — an incomplete row is just visually flagged, not split out.</summary>
    [HttpGet]
    public async Task<IActionResult> Clients(string? key)
    {
        var hasExistingCompany = await _adminContext.Companies.AnyAsync();
        if (hasExistingCompany && !IsSetupKeyValid(key) && !IsMasterLicenseAdmin())
            return View("Locked", !string.IsNullOrEmpty(key));

        var companies = await _adminContext.Companies
            .Include(c => c.Licenses)
            .Include(c => c.ClientDatabases)
            .ToListAsync();

        // Loaded in full rather than aggregated server-side: this table stays small (one row
        // per provisioning attempt, not per request), and grouping "latest attempt per company"
        // via GroupBy().OrderByDescending().First() doesn't translate reliably to SQL anyway.
        var allHistory = await _adminContext.InstallationHistories
            .OrderByDescending(h => h.InstallationDate)
            .ToListAsync();
        var latestHistoryByCompany = allHistory
            .Where(h => h.CompanyId.HasValue)
            .GroupBy(h => h.CompanyId!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        var completedRows = companies.Select(c =>
        {
            var license = c.Licenses.OrderByDescending(l => l.IssueDate).FirstOrDefault();
            var db = c.ClientDatabases.OrderByDescending(d => d.Id).FirstOrDefault();
            latestHistoryByCompany.TryGetValue(c.Id, out var lastHistory);
            return new ClientListItem
            {
                Id = c.Id,
                IsComplete = true,
                CompanyName = c.CompanyName,
                CompanyCode = c.CompanyCode,
                Address = c.Address,
                Country = c.Country,
                State = c.State,
                City = c.City,
                GstNumber = c.GstNumber,
                ContactPerson = c.ContactPerson,
                Email = c.Email,
                Phone = c.Phone,
                InstallationLocation = c.InstallationLocation,
                CompanyStatus = c.Status,
                LicenseNumber = license?.LicenseNumber,
                LicenseType = license?.LicenseType.ToString() ?? "Trial",
                LicenseStatus = license?.Status ?? LicenseStatus.Active,
                ExpiryDate = license?.ExpiryDate,
                Activated = license?.Activated ?? false,
                DatabaseName = db?.DatabaseName,
                DatabaseUsername = db?.DatabaseUsername,
                ServerName = db?.ServerName,
                DatabaseStatus = db?.Status,
                InstallationDate = lastHistory?.InstallationDate ?? c.CreatedAt,
                InstalledBy = lastHistory?.InstalledBy,
                MachineName = lastHistory?.MachineName,
                LastInstallStatus = lastHistory?.InstallationStatus ?? InstallationStatus.Succeeded,
                LastInstallError = lastHistory?.ErrorLog
            };
        });

        var incompleteRows = allHistory
            .Where(h => !h.CompanyId.HasValue)
            .Select(h => new ClientListItem
            {
                Id = h.Id,
                IsComplete = false,
                CompanyName = h.CompanyName ?? "(unnamed attempt)",
                CompanyCode = h.CompanyCode ?? "—",
                Address = h.Address,
                Country = h.Country,
                State = h.State,
                City = h.City,
                GstNumber = h.GstNumber,
                ContactPerson = h.ContactPerson,
                Email = h.Email,
                Phone = h.Phone,
                InstallationLocation = h.InstallationLocation,
                LicenseType = h.LicenseType ?? "Trial",
                DatabaseName = h.DatabaseName,
                DatabaseUsername = h.DatabaseUsername,
                AdminFullName = h.AdminFullName,
                AdminEmail = h.AdminEmail,
                InstallationDate = h.InstallationDate,
                InstalledBy = h.InstalledBy,
                MachineName = h.MachineName,
                LastInstallStatus = h.InstallationStatus,
                LastInstallError = h.ErrorLog
            });

        var model = completedRows.Concat(incompleteRows)
            .OrderByDescending(r => r.InstallationDate)
            .ToList();

        return View(model);
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
