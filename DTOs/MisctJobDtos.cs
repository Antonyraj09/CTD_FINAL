namespace CTD_FINAL.DTOs;

/// <summary>Slim projection for the Job MISCT list — only the columns the row renders,
/// and a container count instead of the full Containers collection (same rationale as
/// PartyListItem: avoid pulling child rows a list view never displays).</summary>
public class MisctJobListItem
{
    public int Id { get; set; }
    public string JobNo { get; set; } = string.Empty;
    public DateTime JobDate { get; set; }
    public string PartyName { get; set; } = string.Empty;
    public string? VesselName { get; set; }
    public string? VoyageNo { get; set; }
    public string? MblNo { get; set; }
    public decimal? GrossWeight { get; set; }
    public string? InvoiceNo { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public int ContainerCount { get; set; }
}
