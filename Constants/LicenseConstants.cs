using CTD_FINAL.Enums;

namespace CTD_FINAL.Constants;

public static class LicenseConstants
{
    public const string LicenseNumberPrefix = "ERC";
    public const string LicenseSequenceKey = "LicenseNo";
    public const string CurrentApplicationVersion = "1.0.0";

    /// <summary>Validity duration by license type, in days, from IssueDate. Config-driven values could override this later; hardcoded defaults here per the spec's explicit "Trial = 30 days."</summary>
    public static readonly Dictionary<LicenseType, int> DurationDays = new()
    {
        [LicenseType.Trial] = 30,
        [LicenseType.Standard] = 365,
        [LicenseType.Professional] = 365,
        [LicenseType.Enterprise] = 365
    };
}
