using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

/// <summary>
/// One address/registration under a <see cref="Party"/>. GST registration in
/// India is issued per state/place of business, so a party operating from
/// more than one state genuinely has more than one GSTIN — this is not
/// optional data modeling, it's how Indian indirect tax registration works.
/// </summary>
public class PartyBranch : BaseEntity
{
    public int PartyId { get; set; }
    public Party Party { get; set; } = null!;

    [Required, StringLength(150)]
    public string BranchName { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;

    [Required, StringLength(300)]
    public string AddressLine1 { get; set; } = string.Empty;

    [StringLength(300)]
    public string? AddressLine2 { get; set; }

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string? State { get; set; }

    [StringLength(15)]
    public string? PinCode { get; set; }

    [StringLength(80)]
    public string Country { get; set; } = "India";

    /// <summary>State-specific GST registration for this place of business.</summary>
    [StringLength(20)]
    public string? Gstin { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(150)]
    public string? ContactPersonName { get; set; }

    /// <summary>Local customs-house/port registration reference for this branch, if any.</summary>
    [StringLength(50)]
    public string? CustomsRegistrationNo { get; set; }
}
