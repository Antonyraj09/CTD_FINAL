using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

/// <summary>
/// "Delivery — ISNE": records physical delivery of a Job ISNE's cargo (one
/// container per delivery, matching how Part (Y/N) works — a multi-container
/// job with a partial delivery gets one Delivery ISNE record per container as
/// each one moves). Most fields are a point-in-time snapshot copied from the
/// Job/Transporter/Staff at delivery time (not a live join) — a Job's freight
/// arrangements can change after a delivery has already gone out, and the
/// delivery record must keep describing what actually happened at the time,
/// not silently follow a later edit to the job.
/// </summary>
public class DeliveryIsne : BaseEntity
{
    /// <summary>Auto-generated on save, continues from the last saved record — never
    /// user-editable. See NumberSequenceService ("DeliveryIsneSerial" key).</summary>
    public int SerialNo { get; set; }

    public DateTime DeliveryDate { get; set; } = DateTime.Today;

    /// <summary>"Y" or "N" — is this a partial delivery of a multi-container job.</summary>
    [Required, StringLength(1)]
    public string PartYN { get; set; } = "N";

    // ---- Job (selected once, then read-only) ----
    public int JobIsneId { get; set; }
    public JobIsne? JobIsne { get; set; }

    [Required, StringLength(30)]
    public string JobNo { get; set; } = string.Empty;

    [StringLength(200)]
    public string? CustomerName { get; set; }

    [StringLength(200)]
    public string? ConsigneeName { get; set; }

    // ---- Delivery details (editable) ----
    [StringLength(40)]
    public string? TruckRailwayReckNo { get; set; }

    [StringLength(60)]
    public string? Shed { get; set; }

    [StringLength(40)]
    public string? KeyNo { get; set; }

    public decimal? Package { get; set; }

    [StringLength(150)]
    public string? Route { get; set; }

    /// <summary>Loose reference to Party.Id where IsTransporter — nullable because a
    /// transporter typed/selected once may later be deleted from the master; the
    /// Code/Name snapshot below is what actually prints on delivery paperwork.</summary>
    public int? TransporterId { get; set; }

    [StringLength(30)]
    public string? TransporterCode { get; set; }

    [StringLength(200)]
    public string? TransporterName { get; set; }

    [StringLength(40)]
    public string? BslNo { get; set; }

    /// <summary>Loose reference to ApplicationUser.Id (internal staff directory).</summary>
    public int? StaffId { get; set; }

    [StringLength(50)]
    public string? StaffCode { get; set; }

    [StringLength(150)]
    public string? StaffName { get; set; }

    [StringLength(15)]
    [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Container Number must be alphanumeric only.")]
    public string? ContainerNo { get; set; }

    [StringLength(10)]
    public string? ContainerSize { get; set; }

    [StringLength(2000)]
    public string? Remarks { get; set; }

    // ---- Soft delete + audit ----
    public bool Deleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    [StringLength(150)]
    public string? DeletedBy { get; set; }

    [StringLength(150)]
    public string? CreatedBy { get; set; }

    [StringLength(150)]
    public string? ModifiedBy { get; set; }
}
