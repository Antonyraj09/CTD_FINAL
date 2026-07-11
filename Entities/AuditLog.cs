using System.ComponentModel.DataAnnotations;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Entities;

public class AuditLog : BaseEntity
{
    [StringLength(30)]
    public string? JobNo { get; set; }

    public int? JobId { get; set; }

    public AuditActionType Action { get; set; }

    [Required, StringLength(150)]
    public string User { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Field { get; set; }

    [StringLength(200)]
    public string? FromValue { get; set; }

    [StringLength(200)]
    public string? ToValue { get; set; }

    [StringLength(500)]
    public string? Detail { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
