using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

public class GeneratedDocument : BaseEntity
{
    [Required, StringLength(250)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string Type { get; set; } = string.Empty;

    public int? CtdJobId { get; set; }
    public CtdJob? CtdJob { get; set; }

    [StringLength(30)]
    public string? JobNo { get; set; }

    [StringLength(150)]
    public string? UploadedBy { get; set; }

    public DateTime DocumentDate { get; set; } = DateTime.UtcNow;

    [StringLength(20)]
    public string? Size { get; set; }

    public bool SystemGenerated { get; set; }

    [StringLength(400)]
    public string? StoragePath { get; set; }
}
