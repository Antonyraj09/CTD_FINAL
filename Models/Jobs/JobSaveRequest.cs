namespace CTD_FINAL.Models.Jobs;

public class JobSaveRequest
{
    public int Id { get; set; }
    public bool CloseJob { get; set; }

    // Step 1
    public DateTime JobDate { get; set; }
    public string ShipmentType { get; set; } = "single";
    public int? ImporterId { get; set; }
    public int? AgentId { get; set; }
    public int? TransporterId { get; set; }
    public string OriginCountry { get; set; } = "India";
    public string? PortArrival { get; set; }
    public int? BorderPointId { get; set; }
    public string? Remarks { get; set; }

    // Step 2
    public string? InvoiceNo { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal InvoiceValue { get; set; }
    public int? CommodityId { get; set; }
    public string? HsCode { get; set; }
    public decimal GrossWt { get; set; }
    public decimal NetWt { get; set; }
    public int Packages { get; set; }
    public List<JobContainerRequest> Containers { get; set; } = new();

    // Step 3
    public string CtdType { get; set; } = "EDI";
    public string? CtdNumber { get; set; }
    public DateTime? CtdDate { get; set; }
    public int? CustomsHouseId { get; set; }
    public int? TransitRouteId { get; set; }
    public DateTime? ExpDeliveryDate { get; set; }
    public List<JobChecklistItemRequest> Checklist { get; set; } = new();

    // Step 4
    public DateTime? ArrivalDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string DeliveryStatus { get; set; } = "In Transit";
    public decimal ServiceCharge { get; set; }
    public decimal TransportCharge { get; set; }
    public decimal OtherCharge { get; set; }
    public decimal TaxPercent { get; set; } = 18;
    public string BillingStatus { get; set; } = "Unpaid";
}

public class JobContainerRequest
{
    public string ContainerNo { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Seal { get; set; }
    public decimal Weight { get; set; }
}

public class JobChecklistItemRequest
{
    public string Name { get; set; } = string.Empty;
    public bool Done { get; set; }
}
