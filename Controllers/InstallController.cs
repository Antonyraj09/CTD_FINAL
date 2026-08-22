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
    private readonly ILicenseService _licenseService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<InstallController> _logger;

    public InstallController(AdminDbContext adminContext, IProvisioningService provisioningService, ILicenseService licenseService, IConfiguration configuration, ILogger<InstallController> logger)
    {
        _adminContext = adminContext;
        _provisioningService = provisioningService;
        _licenseService = licenseService;
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
                prefill = ToPrefill(history);
        }
        else
        {
            // One incomplete attempt has to be resolved (resumed or discarded) before another
            // can start — otherwise retrying with different details just piles up more
            // half-finished attempts instead of finishing the one already in progress.
            var pending = await GetPendingAttemptsAsync();
            if (pending.Count > 0)
            {
                ViewBag.SetupKey = key;
                return View("Pending", pending);
            }
        }

        return View(new InstallIndexViewModel { RequiresSetupKey = hasExistingCompany, SetupKey = key, Prefill = prefill });
    }

    /// <summary>Dismisses an incomplete attempt that has nothing worth resuming (or that the
    /// operator no longer intends to finish) so it stops blocking new installs. The history row
    /// stays for the audit trail, just marked Abandoned instead of Failed.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Discard(int id, string? key)
    {
        if (!IsSetupKeyValid(key) && !IsMasterLicenseAdmin())
            return Forbid();

        var history = await _adminContext.InstallationHistories.FirstOrDefaultAsync(h => h.Id == id && !h.CompanyId.HasValue);
        if (history is not null)
        {
            history.InstallationStatus = InstallationStatus.Abandoned;
            await _adminContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { key });
    }

    /// <summary>Read-only view of every real client provisioned so far — Company joined with its
    /// License/ClientDatabase — same access gate as re-running the wizard itself. Incomplete
    /// attempts are deliberately not shown here; they're surfaced on the Index landing page
    /// instead, since an unresolved one blocks starting a new install anyway.</summary>
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
        var latestHistoryByCompany = (await _adminContext.InstallationHistories
                .Where(h => h.CompanyId.HasValue)
                .OrderByDescending(h => h.InstallationDate)
                .ToListAsync())
            .GroupBy(h => h.CompanyId!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        var model = companies.Select(c =>
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
        })
        .OrderByDescending(r => r.InstallationDate)
        .ToList();

        ViewBag.PendingCount = (await GetPendingAttemptsAsync()).Count;
        ViewBag.SetupKey = key;
        return View(model);
    }

    /// <summary>Edits a provisioned client's Company/License details from the Installed Clients
    /// detail modal. Company Name, Company Code, database connection fields, License Number,
    /// and the installation-date audit stamp aren't accepted here — they stay read-only.
    /// LicenseType/ExpiryDate feed the license's cryptographic signature (see LicenseService),
    /// so changing either re-signs and re-encrypts the license in the same save.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateClient([FromBody] ClientUpdateRequest request)
    {
        var hasExistingCompany = await _adminContext.Companies.AnyAsync();
        if (hasExistingCompany && !IsSetupKeyValid(request.SetupKey) && !IsMasterLicenseAdmin())
            return Json(new { success = false, message = "A valid setup key is required to edit client details." });

        var company = await _adminContext.Companies
            .Include(c => c.Licenses)
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId);
        if (company is null) return Json(new { success = false, message = "Client not found." });

        company.Address = NullIfEmpty(request.Address);
        company.Country = NullIfEmpty(request.Country);
        company.State = NullIfEmpty(request.State);
        company.City = NullIfEmpty(request.City);
        company.GstNumber = NullIfEmpty(request.GstNumber);
        company.ContactPerson = NullIfEmpty(request.ContactPerson);
        company.Email = request.Email?.Trim() ?? string.Empty;
        company.Phone = NullIfEmpty(request.Phone);
        company.InstallationLocation = NullIfEmpty(request.InstallationLocation);
        if (Enum.TryParse<CompanyStatus>(request.CompanyStatus, true, out var companyStatus))
            company.Status = companyStatus;

        var license = company.Licenses.OrderByDescending(l => l.IssueDate).FirstOrDefault();
        if (license is not null)
        {
            var newLicenseType = Enum.TryParse<LicenseType>(request.LicenseType, true, out var lt) ? lt : license.LicenseType;
            var newExpiryDate = request.ExpiryDate ?? license.ExpiryDate;
            var signedFieldsChanged = newLicenseType != license.LicenseType || newExpiryDate != license.ExpiryDate;

            license.LicenseType = newLicenseType;
            license.ExpiryDate = newExpiryDate;
            license.Activated = request.Activated;
            if (Enum.TryParse<LicenseStatus>(request.LicenseStatus, true, out var licenseStatus))
                license.Status = licenseStatus;

            if (signedFieldsChanged)
                _licenseService.ReissueSignature(license, company.CompanyCode);
        }

        var lastHistory = await _adminContext.InstallationHistories
            .Where(h => h.CompanyId == company.Id)
            .OrderByDescending(h => h.InstallationDate)
            .FirstOrDefaultAsync();
        if (lastHistory is not null)
        {
            lastHistory.InstalledBy = NullIfEmpty(request.InstalledBy);
            lastHistory.MachineName = NullIfEmpty(request.MachineName);
        }

        await _adminContext.SaveChangesAsync();
        return Json(new { success = true, message = $"{company.CompanyName} updated successfully" });
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

    /// <summary>Attempts that never reached the point of creating a Company row — Started (the
    /// app never got to record an outcome, e.g. it crashed mid-install) or Failed. Succeeded and
    /// Abandoned are excluded: a succeeded one has a Company by definition, and Abandoned means
    /// the operator already dismissed it.</summary>
    private async Task<List<ClientListItem>> GetPendingAttemptsAsync() =>
        (await _adminContext.InstallationHistories
            .Where(h => !h.CompanyId.HasValue && (h.InstallationStatus == InstallationStatus.Started || h.InstallationStatus == InstallationStatus.Failed))
            .OrderByDescending(h => h.InstallationDate)
            .ToListAsync())
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
        }).ToList();

    private static InstallPrefill ToPrefill(Entities.Admin.InstallationHistory history) => new()
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
