namespace CTD_FINAL.Interfaces;

/// <summary>Reproduces the prototype's nextJobNumber/nextInvoiceNumber/nextDocNumber/nextCtdNumberEDI generators, DB-backed instead of in-memory counters.</summary>
public interface INumberSequenceService
{
    Task<string> NextJobNumberAsync(CancellationToken ct = default);
    Task<string> NextInvoiceNumberAsync(CancellationToken ct = default);
    Task<string> NextDocNumberAsync(string prefix, CancellationToken ct = default);
    Task<string> NextIsneJobNumberAsync(CancellationToken ct = default);

    /// <summary>Job MISCT's own number series — same live-MAX-over-existing-rows convention as
    /// NextIsneJobNumberAsync, just against MisctJobs and the "MISCT/" prefix.</summary>
    Task<string> NextMisctJobNumberAsync(CancellationToken ct = default);

    /// <summary>Delivery ISNE's Serial No. — a plain incrementing integer (no prefix/year),
    /// continuing from the last saved record, per spec.</summary>
    Task<int> NextDeliveryIsneSerialAsync(CancellationToken ct = default);
}
