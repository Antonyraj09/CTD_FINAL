using System.ComponentModel.DataAnnotations;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Entities;

/// <summary>
/// Detailed ERP-style "Job — ISNE" (Import Sea / Nepal Entry) form.
/// Field-for-field mirror of the prototype's five-section static form
/// (Sections A-E) which existed in the UI but had no backing store — this
/// gives it real Create/Edit/Delete/Print functionality.
/// </summary>
public class JobIsne : BaseEntity
{
    // ---- Section A: Job Information ----
    [Required, StringLength(30)]
    public string JobNumber { get; set; } = string.Empty;

    public DateTime JobDate { get; set; }

    [Required, StringLength(30)]
    public string PartyCode { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string PartyName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(30)]
    public string? SubAgentCode { get; set; }

    [StringLength(200)]
    public string? SubAgentName { get; set; }

    /// <summary>Fixed 25-character alphanumeric CTD/transit number — no special characters.</summary>
    [StringLength(25), RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "CTD Number must be alphanumeric only.")]
    public string? CtdNumber { get; set; }

    public DateTime? CtdDate { get; set; }

    [StringLength(30)]
    public string? VesselName { get; set; }

    [StringLength(30)]
    public string? VoyageNo { get; set; }

    [StringLength(30)]
    public string? TsVessel { get; set; }

    [StringLength(30)]
    public string? TsVoyage { get; set; }

    [StringLength(100)]
    public string? CountryCgn { get; set; }

    [StringLength(100)]
    public string? CountryOrigin { get; set; }

    [StringLength(100)]
    public string? RouteOfTransit { get; set; }

    [StringLength(40)]
    public string? RotNo { get; set; }

    public DateTime? RotDate { get; set; }

    public DateTime? InwardDate { get; set; }

    [StringLength(40)]
    public string? LineNo { get; set; }

    // ---- Section B: Document Information ----
    [StringLength(40)]
    public string? MblNo { get; set; }
    public DateTime? MblDate { get; set; }

    [StringLength(40)]
    public string? HblNo { get; set; }
    public DateTime? HblDate { get; set; }

    [StringLength(40)]
    public string? IlNo { get; set; }
    public DateTime? IlDate { get; set; }

    [StringLength(60)]
    public string? LcNo { get; set; }
    public DateTime? LcDate { get; set; }

    [StringLength(100)]
    public string? AccountName { get; set; }

    [StringLength(200)]
    public string? BankName { get; set; }

    [StringLength(60)]
    public string? RefNo { get; set; }
    public DateTime? RefDate { get; set; }

    [StringLength(100)]
    public string? SteamerAgent { get; set; }

    [StringLength(100)]
    public string? ContainerAgent { get; set; }

    // ---- Section C: Transit & Delivery Details ----
    public DateTime? VesselArrival { get; set; }

    [StringLength(150)]
    public string? CtdSentTo { get; set; }

    public bool GreenCtd { get; set; }

    public DateTime? DuePackingList { get; set; }
    public DateTime? DueInvoice { get; set; }
    public DateTime? DueOriginalBl { get; set; }
    public DateTime? DueInsuranceCert { get; set; }
    public DateTime? DueLcCopy { get; set; }
    public DateTime? DueLoa { get; set; }
    public DateTime? DueOrigin { get; set; }
    public DateTime? DueProformaInvoice { get; set; }

    // ---- Section D: Container & Cargo Details ----
    /// <summary>Overall/default shipment type shown in Shipment Information; individual rows may still differ.</summary>
    public ContainerStatus ShipmentType { get; set; } = ContainerStatus.FCL;

    public ICollection<JobIsneContainer> Containers { get; set; } = new List<JobIsneContainer>();

    [StringLength(300)]
    public string? MiscDescription { get; set; }

    [StringLength(1000)]
    public string? CargoDescription { get; set; }

    // ---- Section E: Commercial Information ----
    [StringLength(5)]
    public string Currency { get; set; } = "USD";

    public decimal? ExchangeRate { get; set; }
    public decimal? FobValue { get; set; }
    public decimal? Freight { get; set; }
    public decimal? CifFc { get; set; }
    public decimal? CifFcReference { get; set; }
    public decimal? InsuranceFc { get; set; }
    public decimal? InsuranceValue { get; set; }

    public decimal? InsuranceExRate { get; set; }
    public decimal? InsuranceRate { get; set; }
    public decimal? InsuranceValueInr { get; set; }
    public decimal? CifInr { get; set; }
    public decimal? MarketRate { get; set; }
    public decimal? MarketValueInr { get; set; }
    public decimal? GrossWeight { get; set; }
    public decimal? NetWeight { get; set; }
    public decimal? LcAmount { get; set; }
    public DateTime? ShipmentExpiry { get; set; }

    [StringLength(20)]
    public string PartialShipment { get; set; } = "ALLOWED";

    public decimal? DutyAmount { get; set; }

    [StringLength(150)]
    public string? CreatedBy { get; set; }
}
