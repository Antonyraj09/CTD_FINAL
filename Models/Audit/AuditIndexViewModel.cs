using CTD_FINAL.DTOs;
using CTD_FINAL.Entities;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Models.Audit;

public class AuditIndexViewModel
{
    public PagedResult<AuditLog> Result { get; set; } = new();
    public IReadOnlyList<string> Users { get; set; } = Array.Empty<string>();
    public string? JobNo { get; set; }
    public string? User { get; set; }
    public AuditActionType? Action { get; set; }
}
