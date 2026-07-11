namespace CTD_FINAL.Constants;

/// <summary>
/// Module/action keys for the Role Permission Matrix (Users &amp; Roles screen).
/// Mirrors the prototype's PERMISSION_MATRIX_DEFAULT module list 1:1, plus
/// JobIsne.Manage for the newly-wired Job ISNE screen.
/// </summary>
public static class PermissionKeys
{
    public const string DashboardView = "Dashboard.View";
    public const string CustomerDashboardView = "CustomerDashboard.View";
    public const string JobCreateEdit = "Job.CreateEdit";
    public const string JobClose = "Job.Close";
    public const string TrackingView = "Tracking.View";
    public const string DocumentUpload = "Document.Upload";
    public const string MasterDataManage = "MasterData.Manage";
    public const string InvoiceGenerate = "Invoice.Generate";
    public const string ReportsViewExport = "Reports.ViewExport";
    public const string UsersRolesManage = "UsersRoles.Manage";
    public const string AuditHistoryView = "AuditHistory.View";
    public const string AlertRulesManage = "AlertRules.Manage";
    public const string JobIsneManage = "JobIsne.Manage";

    public static readonly (string Key, string Label)[] All =
    {
        (DashboardView, "Dashboard — View"),
        (CustomerDashboardView, "Customer Dashboard — View"),
        (JobCreateEdit, "Create / Edit CTD Job"),
        (JobClose, "Close CTD Job"),
        (TrackingView, "CTD Tracking — View"),
        (DocumentUpload, "Document Archive — Upload"),
        (MasterDataManage, "Master Data — Manage"),
        (InvoiceGenerate, "Generate Invoice"),
        (ReportsViewExport, "Reports — View & Export"),
        (UsersRolesManage, "Users & Roles — Manage"),
        (AuditHistoryView, "Audit History — View"),
        (AlertRulesManage, "Alert Rules — Manage"),
        (JobIsneManage, "Job ISNE — Manage"),
    };
}
