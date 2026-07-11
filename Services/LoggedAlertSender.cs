using CTD_FINAL.Enums;
using CTD_FINAL.Interfaces;
using Microsoft.Extensions.Logging;

namespace CTD_FINAL.Services;

/// <summary>Default IAlertSender: no SMTP/SMS provider is configured for this environment, so delivery is simulated and recorded to the application log. Swap for a real provider (e.g. SMTP IEmailSender, Twilio) without touching callers.</summary>
public class LoggedAlertSender : IAlertSender
{
    private readonly ILogger<LoggedAlertSender> _logger;

    public LoggedAlertSender(ILogger<LoggedAlertSender> logger) => _logger = logger;

    public Task<string> SendAsync(AlertChannel channel, string to, string message, CancellationToken ct = default)
    {
        _logger.LogInformation("[Simulated {Channel} alert] To: {To} — {Message}", channel, to, message);
        return Task.FromResult("Delivered");
    }
}
