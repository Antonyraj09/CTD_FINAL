using System.ComponentModel.DataAnnotations;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Entities;

/// <summary>
/// One row of the Job ISNE "Container Details" grid. A Job ISNE now owns a
/// collection of these instead of a single set of container fields, so a
/// shipment spanning many containers (or an LCL job with none) can be
/// captured row-by-row while Misc/Cargo Description stay shared on the parent.
/// </summary>
public class JobIsneContainer : BaseEntity
{
    public int JobIsneId { get; set; }
    public JobIsne JobIsne { get; set; } = null!;

    public int SortOrder { get; set; }

    /// <summary>Fixed 15-character alphanumeric container number — no special characters.</summary>
    [StringLength(15)]
    [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Container Number must be alphanumeric only.")]
    public string? ContainerNo { get; set; }

    [StringLength(10)]
    public string ContainerSize { get; set; } = "20ft";

    public ContainerStatus ShipmentType { get; set; } = ContainerStatus.FCL;

    public int NoPackages { get; set; }

    [StringLength(40)]
    public string? PackageType { get; set; }

    public decimal? GrossWeight { get; set; }

    [StringLength(10)]
    public string GrossWeightUnit { get; set; } = "KG";

    public decimal? NetWeight { get; set; }

    [StringLength(10)]
    public string NetWeightUnit { get; set; } = "KG";

    [StringLength(200)]
    public string? MarksSerial { get; set; }

    [StringLength(60)]
    public string? CustomsCode { get; set; }
}
