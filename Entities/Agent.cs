using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

public class Agent : BaseEntity
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string License { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Phone { get; set; }

    [StringLength(150)]
    public string? Email { get; set; }

    public ICollection<CtdJob> Jobs { get; set; } = new List<CtdJob>();
}
