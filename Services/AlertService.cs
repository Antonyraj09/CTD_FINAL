using CTD_FINAL.Entities;
using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using CTD_FINAL.Data;
using Microsoft.EntityFrameworkCore;

namespace CTD_FINAL.Services;

public class AlertService : IAlertService
{
    private readonly AppDbContext _context;
    private readonly IAlertSender _sender;

    public AlertService(AppDbContext context, IAlertSender sender)
    {
        _context = context;
        _sender = sender;
    }

    public async Task<IReadOnlyList<AlertRule>> GetRulesAsync(CancellationToken ct = default) =>
        await _context.AlertRules.AsNoTracking().OrderByDescending(r => r.Id).ToListAsync(ct);

    public async Task<AlertRule> CreateRuleAsync(AlertRule rule, CancellationToken ct = default)
    {
        _context.AlertRules.Add(rule);
        await _context.SaveChangesAsync(ct);
        return rule;
    }

    public async Task<AlertRule?> ToggleRuleAsync(int id, bool active, CancellationToken ct = default)
    {
        var rule = await _context.AlertRules.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rule is null) return null;
        rule.Active = active;
        rule.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return rule;
    }

    public async Task<IReadOnlyList<AlertLog>> GetRecentLogAsync(int count = 40, CancellationToken ct = default) =>
        await _context.AlertLogs.AsNoTracking().OrderByDescending(a => a.SentAt).Take(count).ToListAsync(ct);

    public async Task NotifyAsync(AlertChannel channel, string to, string trigger, string? jobNo, CancellationToken ct = default)
    {
        var status = await _sender.SendAsync(channel, to, $"{trigger}{(jobNo != null ? $" — Job {jobNo}" : "")}", ct);
        _context.AlertLogs.Add(new AlertLog
        {
            Channel = channel,
            To = to,
            Trigger = trigger,
            JobNo = jobNo,
            SentAt = DateTime.UtcNow,
            Status = status
        });
        await _context.SaveChangesAsync(ct);
    }
}
