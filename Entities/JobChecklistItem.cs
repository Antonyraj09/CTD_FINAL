using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

public class JobChecklistItem : BaseEntity
{
    public int CtdJobId { get; set; }
    public CtdJob CtdJob { get; set; } = null!;

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    public bool Done { get; set; }

    public int SortOrder { get; set; }
}
