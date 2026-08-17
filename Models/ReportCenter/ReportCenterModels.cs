namespace CTD_FINAL.Models.ReportCenter;

/// <summary>One matched record, shaped uniformly regardless of which entity it came from —
/// the view renders every category's hits with the same card layout.</summary>
public class ReportCenterHit
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string? Date { get; set; }
    public string Url { get; set; } = string.Empty;
}

/// <summary>All hits found for one entity type (Job ISNE, Party, ...), only present in the
/// response at all when the requesting user's role can view that entity type.</summary>
public class ReportCenterCategory
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public List<ReportCenterHit> Hits { get; set; } = new();
}
