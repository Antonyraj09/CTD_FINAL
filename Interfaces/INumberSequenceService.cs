namespace CTD_FINAL.Interfaces;

/// <summary>Reproduces the prototype's nextJobNumber/nextInvoiceNumber/nextDocNumber/nextCtdNumberEDI generators, DB-backed instead of in-memory counters.</summary>
public interface INumberSequenceService
{
    Task<string> NextJobNumberAsync(CancellationToken ct = default);
    Task<string> NextInvoiceNumberAsync(CancellationToken ct = default);
    Task<string> NextDocNumberAsync(string prefix, CancellationToken ct = default);
    Task<string> NextIsneJobNumberAsync(CancellationToken ct = default);
}
