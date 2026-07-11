using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

public class BorderPoint : BaseEntity
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [StringLength(150)]
    public string? State { get; set; }

    public ICollection<CtdJob> Jobs { get; set; } = new List<CtdJob>();
}
