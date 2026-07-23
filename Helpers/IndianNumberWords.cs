namespace CTD_FINAL.Helpers;

/// <summary>
/// Converts a whole-rupee amount to words using the Indian numbering system
/// (lakh/crore, not thousand/million) — needed for the Undertaking Bond's
/// "(Rupees ... only)" line, which the reference form writes in sentence case
/// (only the first word capitalized) with no "and" between groups, e.g.
/// 3356906 -> "Thirty three lakh fifty six thousand nine hundred six only".
/// </summary>
public static class IndianNumberWords
{
    private static readonly string[] Ones =
    {
        "", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
        "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen",
        "seventeen", "eighteen", "nineteen"
    };

    private static readonly string[] Tens =
    {
        "", "", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"
    };

    public static string ToWords(decimal amount)
    {
        var whole = (long)Math.Round(amount, MidpointRounding.AwayFromZero);
        var words = ConvertWhole(whole);
        return CapitalizeFirst(words) + " only";
    }

    private static string ConvertWhole(long number)
    {
        if (number == 0) return "zero";

        var parts = new List<string>();
        var crore = number / 1_00_00_000;
        number %= 1_00_00_000;
        var lakh = number / 1_00_000;
        number %= 1_00_000;
        var thousand = number / 1_000;
        number %= 1_000;
        var hundred = number / 100;
        var remainder = number % 100;

        if (crore > 0) parts.Add($"{UnderHundred(crore)} crore");
        if (lakh > 0) parts.Add($"{UnderHundred(lakh)} lakh");
        if (thousand > 0) parts.Add($"{UnderHundred(thousand)} thousand");
        if (hundred > 0) parts.Add($"{Ones[hundred]} hundred");
        if (remainder > 0) parts.Add(UnderHundred(remainder));

        return string.Join(" ", parts);
    }

    // Handles values up to a few hundred (crore/lakh/thousand groups can exceed 99
    // for very large amounts), not just the 0-99 the Indian grouping normally leaves.
    private static string UnderHundred(long n)
    {
        if (n < 20) return Ones[n];
        if (n < 100) return Tens[n / 10] + (n % 10 > 0 ? " " + Ones[n % 10] : "");
        return ConvertWhole(n);
    }

    private static string CapitalizeFirst(string s) =>
        s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s[1..];
}
