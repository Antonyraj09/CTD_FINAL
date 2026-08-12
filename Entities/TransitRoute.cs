using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

public class TransitRoute : BaseEntity
{
    /// <summary>Kept in sync with JobIsne.RouteOfTransit's own limit — a route long enough to
    /// be saved here must also fit there, since a job save copies this Name into that field.</summary>
    [Required, StringLength(1000)]
    public string Name { get; set; } = string.Empty;

    [StringLength(50)]
    public string? Distance { get; set; }

    public ICollection<CtdJob> Jobs { get; set; } = new List<CtdJob>();
}
