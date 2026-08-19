using System.Text.RegularExpressions;

namespace CTD_FINAL.Helpers;

/// <summary>Some Party addresses have "EXIM: &lt;code&gt;" embedded as free text (a legacy
/// data-entry habit, not a separate field on JobIsne) — shared by every print view that
/// shows an Importer's Name &amp; Address block.</summary>
public static class PartyAddressHelper
{
    private static readonly Regex EximRegex = new(@"EXIM:\s*([A-Za-z0-9]+)", RegexOptions.IgnoreCase);
    private static readonly Regex EximStripRegex = new(@",?\s*EXIM:\s*[A-Za-z0-9]+", RegexOptions.IgnoreCase);

    /// <summary>Strips the embedded EXIM code from the address for display, and derives a
    /// PAN fallback (the EXIM code's first 9 characters) for use only when the Party has no
    /// PAN of its own on file (knownPan is null/blank).</summary>
    public static (string DisplayAddress, string? Pan) ExtractPan(string? address, string? knownPan)
    {
        var text = address ?? string.Empty;
        var match = EximRegex.Match(text);
        var displayAddress = match.Success ? EximStripRegex.Replace(text, string.Empty).Trim() : text;

        var pan = knownPan;
        if (string.IsNullOrWhiteSpace(pan) && match.Success)
        {
            var eximCode = match.Groups[1].Value;
            pan = eximCode.Length > 9 ? eximCode[..9] : eximCode;
        }

        return (displayAddress, pan);
    }
}
