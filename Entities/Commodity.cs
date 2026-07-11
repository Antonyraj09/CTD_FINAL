using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

public class Commodity : BaseEntity
{
    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string HsCode { get; set; } = string.Empty;

    public ICollection<CtdJob> Jobs { get; set; } = new List<CtdJob>();
}
