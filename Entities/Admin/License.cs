using System.ComponentModel.DataAnnotations;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Entities.Admin;

/// <summary>
/// One issued license per company (a company could in principle hold more than one
/// over its lifetime, e.g. a Trial followed by a Professional purchase — LicenseNumber
/// is the durable identifier a client types into the login screen).
/// </summary>
public class License : BaseEntity
{
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    /// <summary>Fixed "ERC" prefix + zero-padded sequence, e.g. ERC00001.</summary>
    [Required, StringLength(20)]
    public string LicenseNumber { get; set; } = string.Empty;

    public LicenseType LicenseType { get; set; } = LicenseType.Trial;

    public DateTime IssueDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    /// <summary>Bound to the first machine that successfully activates this license; null until then.</summary>
    [StringLength(200)]
    public string? MachineIdentifier { get; set; }

    public DateTime InstallationDate { get; set; }

    public LicenseStatus Status { get; set; } = LicenseStatus.Active;

    public bool Activated { get; set; }

    public DateTime? LastValidation { get; set; }

    /// <summary>RSA digital signature (base64) over the license payload — proves authenticity/detects tampering.</summary>
    [StringLength(1000)]
    public string? LicenseKey { get; set; }

    /// <summary>AES-encrypted (base64) copy of the full license payload — confidentiality at rest.</summary>
    [StringLength(2000)]
    public string? EncryptedLicense { get; set; }

    [StringLength(20)]
    public string? ApplicationVersion { get; set; }
}
