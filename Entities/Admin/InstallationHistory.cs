using System.ComponentModel.DataAnnotations;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Entities.Admin;

/// <summary>
/// One row per Step 3 provisioning attempt (not one per company) — a failed attempt
/// followed by a retry leaves both in the audit trail rather than overwriting.
///
/// Carries a full (password-free) copy of the request fields, written before the risky
/// database/schema steps run — so an attempt that fails before a Company row ever exists
/// still has enough to re-populate the wizard and let the operator resume where they left
/// off, instead of retyping everything. Database/Administrator passwords are deliberately
/// never persisted here (or anywhere in ADMIN_CTD) — the operator re-enters them on resume,
/// same as a fresh install.
/// </summary>
public class InstallationHistory : BaseEntity
{
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

    public DateTime InstallationDate { get; set; }

    [StringLength(150)]
    public string? InstalledBy { get; set; }

    [StringLength(150)]
    public string? MachineName { get; set; }

    [StringLength(20)]
    public string? ApplicationVersion { get; set; }

    [StringLength(50)]
    public string? DatabaseVersion { get; set; }

    public InstallationStatus InstallationStatus { get; set; } = InstallationStatus.Started;

    [StringLength(4000)]
    public string? ErrorLog { get; set; }

    // ---- Request snapshot (for the Installed Clients screen's detail view and Resume flow) ----
    [StringLength(200)]
    public string? CompanyName { get; set; }

    [StringLength(20)]
    public string? CompanyCode { get; set; }

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

    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(200)]
    public string? InstallationLocation { get; set; }

    [StringLength(20)]
    public string? LicenseType { get; set; }

    [StringLength(63)]
    public string? DatabaseName { get; set; }

    [StringLength(63)]
    public string? DatabaseUsername { get; set; }

    [StringLength(150)]
    public string? AdminFullName { get; set; }

    [StringLength(200)]
    public string? AdminEmail { get; set; }
}
