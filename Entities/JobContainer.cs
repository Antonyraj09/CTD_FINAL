using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

public class JobContainer : BaseEntity
{
    public int CtdJobId { get; set; }
    public CtdJob CtdJob { get; set; } = null!;

    [Required, StringLength(30)]
    public string ContainerNo { get; set; } = string.Empty;

    [StringLength(40)]
    public string? Size { get; set; }

    [StringLength(30)]
    public string? Seal { get; set; }

    public decimal Weight { get; set; }
}
