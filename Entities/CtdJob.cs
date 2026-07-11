using System.ComponentModel.DataAnnotations;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Entities;

/// <summary>
/// The core CTD transactional job entity (backs the "New CTD Job" 4-step wizard
/// and the CTD Tracking screen). Field-for-field mirror of the prototype's
/// gatherJobFromForm()/seedJobs() job object.
/// </summary>
public class CtdJob : BaseEntity
{
    [Required, StringLength(30)]
    public string JobNo { get; set; } = string.Empty;

    public DateTime JobDate { get; set; }

    // Step 1: Job Information
    public int? ImporterId { get; set; }
    public Party? Importer { get; set; }

    public int? AgentId { get; set; }
    public Party? Agent { get; set; }

    public int? TransporterId { get; set; }
    public Party? Transporter { get; set; }

    [StringLength(80)]
    public string OriginCountry { get; set; } = "India";

    [StringLength(80)]
    public string? PortArrival { get; set; }

    public int? BorderPointId { get; set; }
    public BorderPoint? BorderPoint { get; set; }

    public ShipmentType ShipmentType { get; set; } = ShipmentType.Single;

    [StringLength(150)]
    public string? CreatedBy { get; set; }

    [StringLength(500)]
    public string? Remarks { get; set; }

    // Step 2: Invoice & Cargo Details
    [StringLength(40)]
    public string? InvoiceNo { get; set; }

    public DateTime? InvoiceDate { get; set; }

    [StringLength(10)]
    public string Currency { get; set; } = "USD";

    public decimal InvoiceValue { get; set; }

    public int? CommodityId { get; set; }
    public Commodity? Commodity { get; set; }

    [StringLength(20)]
    public string? HsCode { get; set; }

    public decimal GrossWt { get; set; }
    public decimal NetWt { get; set; }
    public int Packages { get; set; }

    public ICollection<JobContainer> Containers { get; set; } = new List<JobContainer>();

    // Step 3: Customs & CTD Processing
    public CtdType CtdType { get; set; } = CtdType.EDI;

    [StringLength(40)]
    public string? CtdNumber { get; set; }

    public DateTime? CtdDate { get; set; }

    public int? CustomsHouseId { get; set; }
    public CustomsHouse? CustomsHouse { get; set; }

    public int? TransitRouteId { get; set; }
    public TransitRoute? TransitRoute { get; set; }

    public DateTime? ExpDeliveryDate { get; set; }

    public ICollection<JobChecklistItem> ChecklistItems { get; set; } = new List<JobChecklistItem>();

    public bool CtdDocGenerated { get; set; }
    public bool ChecklistDocGenerated { get; set; }
    public bool ForwardingDocGenerated { get; set; }

    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;

    // Step 4: Delivery & Billing
    public DateTime? ArrivalDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DeliveryStatusType DeliveryStatus { get; set; } = DeliveryStatusType.InTransit;

    public decimal ServiceCharge { get; set; }
    public decimal TransportCharge { get; set; }
    public decimal OtherCharge { get; set; }
    public decimal TaxPercent { get; set; } = 18;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }
    public BillingStatus BillingStatus { get; set; } = BillingStatus.Unpaid;

    public bool InvoiceGenerated { get; set; }
}
