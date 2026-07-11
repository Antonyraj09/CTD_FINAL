using System.ComponentModel.DataAnnotations;

namespace CTD_FINAL.Entities;

/// <summary>Backs the running counters used by INumberSequenceService (job/invoice/document/CTD numbers).</summary>
public class NumberSequence : BaseEntity
{
    [Required, StringLength(30)]
    public string Key { get; set; } = string.Empty;

    public int CurrentValue { get; set; }
}
