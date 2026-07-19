using System.ComponentModel.DataAnnotations;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Entities.Admin;

/// <summary>
/// One row per Step 3 provisioning attempt (not one per company) — a failed attempt
/// followed by a retry leaves both in the audit trail rather than overwriting.
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
}
