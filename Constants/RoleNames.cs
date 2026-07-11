namespace CTD_FINAL.Constants;

/// <summary>
/// Application roles. Names follow the spec's generic naming; each carries the
/// prototype's original permission set 1:1 (Administrator=Customs Admin,
/// Manager=Operations User, Operator=Accounts User, Viewer=Customer).
/// </summary>
public static class RoleNames
{
    public const string Administrator = "Administrator";
    public const string Manager = "Manager";
    public const string Operator = "Operator";
    public const string Viewer = "Viewer";

    public static readonly string[] All = { Administrator, Manager, Operator, Viewer };
}
