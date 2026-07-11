using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Models.Alerts;

public class AlertRuleFormViewModel
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Channel { get; set; } = "Email";

    [StringLength(200)]
    public string? Trigger { get; set; }

    [StringLength(150)]
    public string? Audience { get; set; }
}
