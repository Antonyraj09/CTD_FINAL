using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

/// <summary>
/// "Job — MISCT" (Miscellaneous Cargo Transit) entry — the legacy Windows ERP screen's
/// digitized replacement, built on the same conventions as <see cref="JobIsne"/>: free-text
/// Party/Sub Agent *snapshots* (not FK dropdowns) copied in at save time from the Party/Sub
/// Agent masters, plus real FK references to the existing Customs House and (Nepal) Border
/// Point masters for the two ends of the transit crossing this form asks for.
/// </summary>
public class MisctJob : BaseEntity
{
    // ---- Job Information ----
    [Required, StringLength(30)]
    public string JobNo { get; set; } = string.Empty;

    public DateTime JobDate { get; set; }

    // ---- Party & Sub Agent (snapshot from Party/SubAgent master, same convention as JobIsne) ----
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

    // ---- Vessel & Movement ----
    [StringLength(30)]
    public string? VesselName { get; set; }

    [StringLength(30)]
    public string? VoyageNo { get; set; }

    [StringLength(100)]
    public string? CountryCgn { get; set; }

    [StringLength(40)]
    public string? RotNo { get; set; }

    public DateTime? RotDate { get; set; }

    [StringLength(40)]
    public string? LineNo { get; set; }

    [StringLength(40)]
    public string? MblNo { get; set; }

    public DateTime? MblDate { get; set; }

    // ---- Customs & Route (real FK to existing masters, snapshot alongside for print stability) ----
    public int? CustomsStationExitId { get; set; }
    public CustomsHouse? CustomsStationExit { get; set; }

    [StringLength(200)]
    public string? CustomsStationExitName { get; set; }

    public int? PortOfEntryNepalId { get; set; }
    public BorderPoint? PortOfEntryNepal { get; set; }

    [StringLength(150)]
    public string? PortOfEntryNepalName { get; set; }

    /// <summary>The Indian seaport where the ocean vessel discharges (e.g. "KOLKATA") — distinct
    /// from CustomsStationExit (the land border crossing, e.g. Raxaul) and PortOfEntryNepal (the
    /// Nepal-side border point, e.g. Birgunj). Free text, not a master reference — needed by the
    /// Transhipment Permit / Declaration of Transshipment print reports.</summary>
    [StringLength(100)]
    public string? PortOfEntryIndia { get; set; }

    /// <summary>Customs bond reference debited for this transit movement — printed on the
    /// Declaration of Transshipment and Transhipment Permit reports.</summary>
    [StringLength(30)]
    public string? BondNo { get; set; }

    // ---- Container / Cargo Information ----
    /// <summary>Header-level count of 20 FT containers on this job — auto-tallied client-side
    /// from the Container Details grid as rows are added/edited, but stored (and editable) as
    /// a real column since the legacy screen treats it as directly meaningful data.</summary>
    public int Container20Qty { get; set; }

    public int Container40Qty { get; set; }

    public int LclQty { get; set; }

    [StringLength(60)]
    public string? CustomCode { get; set; }

    public int NoOfPackage { get; set; }

    /// <summary>Free-text descriptive summary, e.g. "2X20' CONT. STC 82 PLT" — JS suggests a
    /// default composed from the container/quantity fields, but the user can freely overwrite it.</summary>
    [StringLength(200)]
    public string? Unit { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public decimal? GrossWeight { get; set; }

    [StringLength(20)]
    public string? InvoiceNo { get; set; }

    public DateTime? InvoiceDate { get; set; }

    // ---- Authorised Carrier (no existing master carries this — plain header fields) ----
    [StringLength(200)]
    public string? CarrierName { get; set; }

    [StringLength(300)]
    public string? CarrierAddress { get; set; }

    [StringLength(20)]
    public string? CarrierGstin { get; set; }

    // ---- Quantity (distinct from No. of Package — e.g. "82 PLT" pallets, not container count) ----
    public int? Quantity { get; set; }

    [StringLength(20)]
    public string? QuantityUnit { get; set; }

    public ICollection<MisctJobContainer> Containers { get; set; } = new List<MisctJobContainer>();

    [StringLength(150)]
    public string? CreatedBy { get; set; }
}
