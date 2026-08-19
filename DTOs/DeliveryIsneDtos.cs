namespace CTD_FINAL.DTOs;

public class DeliveryIsneFilter
{
    public string? SerialNo { get; set; }
    public string? JobNo { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Customer { get; set; }
    public string? Transporter { get; set; }
    public string? Quick { get; set; }
    public string SortKey { get; set; } = "deliveryDate";
    public string SortDir { get; set; } = "desc";
}

/// <summary>Shape posted by wwwroot/js/delivery-isne.js when saving the Entry screen.</summary>
public class DeliveryIsneSaveRequest
{
    public int Id { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string PartYN { get; set; } = "N";
    public int JobIsneId { get; set; }

    public string? TruckRailwayReckNo { get; set; }
    public string? Shed { get; set; }
    public string? KeyNo { get; set; }
    public decimal? Package { get; set; }
    public string? Route { get; set; }

    public int? TransporterId { get; set; }
    public string? TransporterCode { get; set; }
    public string? TransporterName { get; set; }

    public string? BslNo { get; set; }

    public int? StaffId { get; set; }
    public string? StaffCode { get; set; }
    public string? StaffName { get; set; }

    public string? ContainerNo { get; set; }
    public string? ContainerSize { get; set; }

    [System.ComponentModel.DataAnnotations.StringLength(2000)]
    public string? Remarks { get; set; }

    public string? ConsigneeName { get; set; }
}
