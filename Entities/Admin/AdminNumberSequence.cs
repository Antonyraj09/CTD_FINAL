using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities.Admin;

/// <summary>Backs the running counter used to generate License Numbers (e.g. ERC00001) — same shape as the tenant-side NumberSequence, but lives in ADMIN_CTD since license numbers are global across all companies.</summary>
public class AdminNumberSequence : BaseEntity
{
    [Required, StringLength(30)]
    public string Key { get; set; } = string.Empty;

    public int CurrentValue { get; set; }
}
