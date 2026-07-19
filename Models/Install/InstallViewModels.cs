using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Models.Install;

/// <summary>What the Install Wizard's landing page (GET /Install) needs to render — whether
/// a setup key is required (any company already exists) and whether one was supplied.</summary>
public class InstallIndexViewModel
{
    public bool RequiresSetupKey { get; set; }
    public string? SetupKey { get; set; }
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

    [Required(ErrorMessage = "Administrator password is required"), StringLength(100, MinimumLength = 8)]
    public string AdminPassword { get; set; } = string.Empty;

    /// <summary>Only required once a company already exists (re-running the wizard against a live install) — see InstallController.IsSetupKeyValid.</summary>
    public string? SetupKey { get; set; }
}
