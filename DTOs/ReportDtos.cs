namespace CTD_FINAL.DTOs;

public class ReportResult
{
    public string Title { get; set; } = string.Empty;
    public IReadOnlyList<string> Columns { get; set; } = Array.Empty<string>();
    public IReadOnlyList<string[]> Rows { get; set; } = Array.Empty<string[]>();
}
