using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

/// <summary>
/// One row of the Job MISCT "Container Details" grid — Container No / Seal No / Weight /
/// CIF Value, per the legacy screen. Same shape as <see cref="JobIsneContainer"/> plus a
/// per-container CIF Value, which Job ISNE's own container grid has no equivalent of.
/// </summary>
public class MisctJobContainer : BaseEntity
{
    public int MisctJobId { get; set; }
    public MisctJob MisctJob { get; set; } = null!;

    public int SortOrder { get; set; }

    /// <summary>Fixed 15-character alphanumeric container number — no special characters.</summary>
    [StringLength(15)]
    [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Container Number must be alphanumeric only.")]
    public string? ContainerNo { get; set; }

    [StringLength(50)]
    public string? SealNo { get; set; }

    /// <summary>"20ft" / "40ft" / "lcl" — same string convention as JobIsneContainer.ContainerSize,
    /// so the header's Container20Qty/Container40Qty/LclQty tally can be computed uniformly.</summary>
    [StringLength(10)]
    public string ContainerSize { get; set; } = "20ft";

    public decimal? Weight { get; set; }

    public decimal? CifValue { get; set; }
}
