namespace CTD_FINAL.DTOs;

public class JobContainerDto
{
    public string ContainerNo { get; set; } = string.Empty;
    public string? Size { get; set; }
    public string? Seal { get; set; }
    public decimal Weight { get; set; }
}

public class JobChecklistItemDto
{
    public string Name { get; set; } = string.Empty;
    public bool Done { get; set; }
}

public class TrackingFilter
{
    public string? JobNo { get; set; }
    public string? CtdNo { get; set; }
    public int? ImporterId { get; set; }
    public string? Container { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Status { get; set; }
    public string? Quick { get; set; }
    public string SortKey { get; set; } = "jobDate";
    public string SortDir { get; set; } = "desc";
}
