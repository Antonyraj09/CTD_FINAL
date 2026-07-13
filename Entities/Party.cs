using System.ComponentModel.DataAnnotations;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Entities;

/// <summary>
/// Unified business-partner master, replacing the three single-role
/// Importer/Agent/Transporter masters. One company can be tagged with any
/// combination of roles via IsImporter/IsTransporter/IsAgent instead of
/// needing a separate record (and separate identity) in three different
/// tables, and can carry any number of branches (<see cref="PartyBranch"/>)
/// — GSTIN registration in India is issued per state/place of business, so
/// a party with operations in multiple states genuinely needs more than one
/// address and more than one GSTIN.
/// </summary>
public class Party : BaseEntity
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>"Trading as" name, if different from the legal name.</summary>
    [StringLength(200)]
    public string? TradeName { get; set; }

    public PartyConstitution Constitution { get; set; } = PartyConstitution.PrivateLimited;

    [StringLength(10)]
    public string? Pan { get; set; }

    /// <summary>DGFT-issued Import Export Code — required for any importer/exporter of record in India.</summary>
    [StringLength(15)]
    public string? IecCode { get; set; }

    /// <summary>Corporate Identification Number, for registered companies.</summary>
    [StringLength(30)]
    public string? CinNumber { get; set; }

    public bool IsImporter { get; set; }
    public bool IsTransporter { get; set; }
    public bool IsAgent { get; set; }

    /// <summary>Agent (CHA)-specific: customs broker license.</summary>
    [StringLength(50)]
    public string? License { get; set; }

    public DateTime? LicenseValidUpto { get; set; }

    /// <summary>Transporter-specific.</summary>
    [StringLength(100)]
    public string? Fleet { get; set; }

    /// <summary>The local customs clearing sub-agent this party normally works through.
    /// A loose reference to SubAgent.SubAgentCode (not an FK), matching the same
    /// free-text-tag convention JobIsne uses for SubAgentCode/PartyName.</summary>
    [StringLength(20)]
    public string? SubAgentCode { get; set; }

    /// <summary>CBIC Authorized Economic Operator status — grants faster customs clearance.</summary>
    public AeoStatus AeoStatus { get; set; } = AeoStatus.None;

    [StringLength(50)]
    public string? AeoCertificateNo { get; set; }

    [StringLength(150)]
    public string? BankName { get; set; }

    [StringLength(30)]
    public string? BankAccountNo { get; set; }

    [StringLength(15)]
    public string? BankIfsc { get; set; }

    /// <summary>Authorized Dealer Code — bank-issued code required on shipping bills/customs forex declarations.</summary>
    [StringLength(20)]
    public string? AdCode { get; set; }

    [StringLength(200)]
    public string? Website { get; set; }

    [StringLength(150)]
    public string? ContactPersonName { get; set; }

    [StringLength(100)]
    public string? ContactPersonDesignation { get; set; }

    [StringLength(30)]
    public string? ContactPersonPhone { get; set; }

    [StringLength(150)]
    public string? ContactPersonEmail { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(1000)]
    public string? Remarks { get; set; }

    public ICollection<PartyBranch> Branches { get; set; } = new List<PartyBranch>();

    public string Roles => string.Join(", ", new[]
    {
        IsImporter ? "Importer" : null,
        IsTransporter ? "Transporter" : null,
        IsAgent ? "Agent" : null
    }.Where(r => r != null));
}
