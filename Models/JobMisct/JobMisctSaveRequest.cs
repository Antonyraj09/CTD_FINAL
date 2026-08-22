namespace CTD_FINAL.Models.JobMisct;

public class JobMisctSaveRequest
{
    public int Id { get; set; }

    // Job Information
    public DateTime JobDate { get; set; }

    // Party & Sub Agent
    public string PartyCode { get; set; } = string.Empty;
    public string PartyName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? SubAgentCode { get; set; }
    public string? SubAgentName { get; set; }

    // Vessel & Movement
    public string? VesselName { get; set; }
    public string? VoyageNo { get; set; }
    public string? CountryCgn { get; set; }
    public string? RotNo { get; set; }
    public DateTime? RotDate { get; set; }
    public string? LineNo { get; set; }
    public string? MblNo { get; set; }
    public DateTime? MblDate { get; set; }

    // Customs & Route
    public int? CustomsStationExitId { get; set; }
    public int? PortOfEntryNepalId { get; set; }
    public string? PortOfEntryIndia { get; set; }
    public string? BondNo { get; set; }

    // Container / Cargo
    public int Container20Qty { get; set; }
    public int Container40Qty { get; set; }
    public int LclQty { get; set; }
    public string? CustomCode { get; set; }
    public int NoOfPackage { get; set; }
    public string? Unit { get; set; }
    public string? Description { get; set; }
    public decimal? GrossWeight { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime? InvoiceDate { get; set; }

    // Authorised Carrier
    public string? CarrierName { get; set; }
    public string? CarrierAddress { get; set; }
    public string? CarrierGstin { get; set; }

    // Quantity
    public int? Quantity { get; set; }
    public string? QuantityUnit { get; set; }

    public List<MisctJobContainerRequest> Containers { get; set; } = new();
}

public class MisctJobContainerRequest
{
    public string? ContainerNo { get; set; }
    public string? SealNo { get; set; }
    public string ContainerSize { get; set; } = "20ft";
    public decimal? Weight { get; set; }
    public decimal? CifValue { get; set; }
}
