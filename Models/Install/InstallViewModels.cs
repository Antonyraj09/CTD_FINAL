using System.ComponentModel.DataAnnotations;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Models.Install;

/// <summary>What the Install Wizard's landing page (GET /Install) needs to render — whether
/// a setup key is required (any company already exists) and whether one was supplied, plus
/// (when opened via "Resume" from the Installed Clients screen) the fields to prefill.</summary>
public class InstallIndexViewModel
{
    public bool RequiresSetupKey { get; set; }
    public string? SetupKey { get; set; }
    public InstallPrefill? Prefill { get; set; }
}

/// <summary>The non-sensitive fields of a previously-failed attempt, read back from
/// InstallationHistory to repopulate Steps 1-2 of the wizard on Resume — everything except
/// Database/Administrator passwords, which are never persisted and must be re-entered.</summary>
public class InstallPrefill
{
    public string? CompanyName { get; set; }
    public string? CompanyCode { get; set; }
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? GstNumber { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? InstallationLocation { get; set; }
    public string? LicenseType { get; set; }
    public string? DatabaseName { get; set; }
    public string? DatabaseUsername { get; set; }
    public string? AdminFullName { get; set; }
    public string? AdminEmail { get; set; }
}

/// <summary>Everything the wizard's three data-entry steps collect, posted as JSON to
/// InstallController.Provision and mapped 1:1 into a ProvisioningRequest.</summary>
public class InstallProvisionRequest
{
    [Required(ErrorMessage = "Company name is required"), StringLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Company code is required"), StringLength(20)]
    [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "Company code may only contain letters, digits, hyphens and underscores")]
    public string CompanyCode { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(100)]
    public string? State { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(20)]
    public string? GstNumber { get; set; }

    [StringLength(150)]
    public string? ContactPerson { get; set; }

    [Required(ErrorMessage = "Company email is required"), EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(200)]
    public string? InstallationLocation { get; set; }

    // A string, not the LicenseType enum directly — System.Text.Json has no
    // JsonStringEnumConverter configured in this app, so a JSON body of "Trial" would fail
    // to bind against an enum property (it expects the numeric value). Parsed in the
    // controller instead, matching JobSaveRequest's ShipmentType/CtdType string convention.
    [Required(ErrorMessage = "License type is required")]
    public string LicenseType { get; set; } = "Trial";

    [Required(ErrorMessage = "Database name is required"), StringLength(63)]
    [RegularExpression(@"^[A-Za-z][A-Za-z0-9_]{2,62}$", ErrorMessage = "Database name must start with a letter and contain only letters, digits and underscores")]
    public string DatabaseName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Database username is required"), StringLength(63)]
    [RegularExpression(@"^[A-Za-z][A-Za-z0-9_]{2,62}$", ErrorMessage = "Database username must start with a letter and contain only letters, digits and underscores")]
    public string DatabaseUsername { get; set; } = string.Empty;

    [Required(ErrorMessage = "Database password is required"), StringLength(100, MinimumLength = 8)]
    public string DatabasePassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Administrator name is required"), StringLength(150)]
    public string AdminFullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Administrator email is required"), EmailAddress, StringLength(200)]
    public string AdminEmail { get; set; } = string.Empty;

    // Must match the Identity password policy TenantSeeder's UserManager actually enforces
    // (Program.cs/TenantSeeder.cs: RequireUppercase/RequireDigit/RequireNonAlphanumeric, plus
    // Identity's own RequireLowercase default) — checking only length here let a password
    // through that was guaranteed to fail deep inside TenantSeeder.SeedDefaultAdminAsync,
    // after the database, login, and full schema had already been created for nothing.
    [Required(ErrorMessage = "Administrator password is required"), StringLength(100, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
        ErrorMessage = "Administrator password must be at least 8 characters and include an uppercase letter, a lowercase letter, a digit, and a symbol.")]
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>Only required once a company already exists (re-running the wizard against a live install) — see InstallController.IsSetupKeyValid.</summary>
    public string? SetupKey { get; set; }
}

/// <summary>One row of either the "Installed Clients" list (a real Company joined with its
/// License/ClientDatabase — <see cref="IsComplete"/> true) or the "pending installation" block
/// screen (an attempt with no Company yet, fields read back from InstallationHistory's request
/// snapshot instead — <see cref="IsComplete"/> false). Both screens use the same shape since
/// most fields overlap, even though only one of them is ever populated for a given row.</summary>
public class ClientListItem
{
    /// <summary>CompanyId when complete, InstallationHistory.Id when not — id space differs
    /// by row kind, so callers must branch on <see cref="IsComplete"/> before using this.</summary>
    public int Id { get; set; }
    public bool IsComplete { get; set; }

    public string CompanyName { get; set; } = string.Empty;
    public string CompanyCode { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Country { get; set; }
    public string? State { get; set; }
    public string? City { get; set; }
    public string? GstNumber { get; set; }
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? InstallationLocation { get; set; }

    public CompanyStatus? CompanyStatus { get; set; }

    public string? LicenseNumber { get; set; }
    public string LicenseType { get; set; } = "Trial";
    public LicenseStatus? LicenseStatus { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool Activated { get; set; }

    public string? DatabaseName { get; set; }
    public string? DatabaseUsername { get; set; }
    public string? ServerName { get; set; }
    public ClientDatabaseStatus? DatabaseStatus { get; set; }

    public string? AdminFullName { get; set; }
    public string? AdminEmail { get; set; }

    public DateTime InstallationDate { get; set; }
    public string? InstalledBy { get; set; }
    public string? MachineName { get; set; }
    public InstallationStatus LastInstallStatus { get; set; }
    public string? LastInstallError { get; set; }
}
