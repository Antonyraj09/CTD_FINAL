using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

public class TransitRoute : BaseEntity
{
    [Required, StringLength(250)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Distance { get; set; }

    public ICollection<CtdJob> Jobs { get; set; } = new List<CtdJob>();
}
