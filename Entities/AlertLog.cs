using System.ComponentModel.DataAnnotations;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Entities;

public class AlertLog : BaseEntity
{
    public int? AlertRuleId { get; set; }
    public AlertRule? AlertRule { get; set; }

    public AlertChannel Channel { get; set; }

    [Required, StringLength(200)]
    public string To { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Trigger { get; set; } = string.Empty;

    [StringLength(30)]
    public string? JobNo { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    [StringLength(30)]
    public string Status { get; set; } = "Sent";
}
