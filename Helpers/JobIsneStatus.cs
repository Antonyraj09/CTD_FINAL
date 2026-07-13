using System.Linq.Expressions;
using CTD_FINAL.Entities;

namespace CTD_FINAL.Helpers;

/// <summary>
/// JobIsne has no WorkflowStatus enum — this derives an honest 3-state pseudo-status
/// from the fields it actually has (CtdNumber / VesselArrival presence).
/// </summary>
public static class JobIsneStatus
{
    public const string PendingCtd = "Pending CTD";
    public const string CtdIssued = "CTD Issued";
    public const string Arrived = "Arrived";

    public static Expression<Func<JobIsne, bool>> IsPendingCtd => j => string.IsNullOrEmpty(j.CtdNumber);
    public static Expression<Func<JobIsne, bool>> IsCtdIssued => j => !string.IsNullOrEmpty(j.CtdNumber) && j.VesselArrival == null;
    public static Expression<Func<JobIsne, bool>> IsArrived => j => j.VesselArrival != null;

    public static string Label(JobIsne j) =>
        j.VesselArrival != null ? Arrived
        : !string.IsNullOrEmpty(j.CtdNumber) ? CtdIssued
        : PendingCtd;

    public static string BadgeClass(string status) => status switch
    {
        Arrived => "badge-delivered",
        CtdIssued => "badge-approved",
        _ => "badge-draft"
    };
}
