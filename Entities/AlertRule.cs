using System.ComponentModel.DataAnnotations;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Entities;

public class AlertRule : BaseEntity
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    public AlertChannel Channel { get; set; } = AlertChannel.Email;

    [Required, StringLength(200)]
    public string Trigger { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Audience { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    public ICollection<AlertLog> Logs { get; set; } = new List<AlertLog>();
}
