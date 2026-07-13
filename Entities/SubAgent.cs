using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

public class SubAgent : BaseEntity
{
    [Required, StringLength(20)]
    public string SubAgentCode { get; set; } = string.Empty;

    // Named "Name" (not "SubAgentName") to match the generic Masters framework's
    // convention — MastersController/_MasterTable.cshtml key their audit-log text,
    // save/delete toasts, and bold "primary column" styling off a property literally
    // named Name across every master tab.
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(250)]
    public string AddressLine1 { get; set; } = string.Empty;

    [StringLength(250)]
    public string? AddressLine2 { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? State { get; set; }

    [StringLength(12)]
    public string? PinCode { get; set; }

    // CHA / customs broker license number — sub-agents clearing cargo on the
    // party's behalf are expected to carry one for customs filings.
    [StringLength(50)]
    public string? LicenseNo { get; set; }

    [StringLength(10)]
    public string? PanNo { get; set; }

    [StringLength(15)]
    public string? GstinNo { get; set; }

    [StringLength(150)]
    public string? ContactPersonName { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }
}
