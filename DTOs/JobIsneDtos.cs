namespace CTD_FINAL.DTOs;

public class JobIsneTrackingFilter
{
    public string? JobNo { get; set; }
    public string? CtdNo { get; set; }
    public string? PartyName { get; set; }
    public string? Container { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Status { get; set; }
    public string? Quick { get; set; }
    public string SortKey { get; set; } = "jobDate";
    public string SortDir { get; set; } = "desc";
}
