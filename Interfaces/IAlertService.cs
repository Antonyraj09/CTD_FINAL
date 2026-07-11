using CTD_FINAL.Entities;
using CTD_FINAL.Enums;

namespace CTD_FINAL.Interfaces;

public interface IAlertService
{
    Task<IReadOnlyList<AlertRule>> GetRulesAsync(CancellationToken ct = default);
    Task<AlertRule> CreateRuleAsync(AlertRule rule, CancellationToken ct = default);
    Task<AlertRule?> ToggleRuleAsync(int id, bool active, CancellationToken ct = default);
    Task<IReadOnlyList<AlertLog>> GetRecentLogAsync(int count = 40, CancellationToken ct = default);

    /// <summary>Fires a notification for a job event (e.g. status change) — records to AlertLog via IAlertSender.</summary>
    Task NotifyAsync(AlertChannel channel, string to, string trigger, string? jobNo, CancellationToken ct = default);
}

/// <summary>
/// Sends (or, absent a configured provider, simply records) a notification.
/// The prototype's Alerts screen never wired a real email/SMS provider either —
/// this default implementation logs to AlertLog exactly as the prototype's
/// in-memory ALERT_LOG did, and can be swapped for a real SMTP/SMS sender later.
/// </summary>
public interface IAlertSender
{
    Task<string> SendAsync(AlertChannel channel, string to, string message, CancellationToken ct = default);
}
