using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Contracts.Notifications;
using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Enums;
using Opti_Sec_Backend.Hubs;
using Opti_Sec_Backend.Persistence;

namespace Opti_Sec_Backend.Services.NotificationServices;

public class NotificationService(
    ApplicationDbContext context,
    IHubContext<GateHub, IGateHubClient> hubContext,
    ILogger<NotificationService> logger) : INotificationService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IHubContext<GateHub, IGateHubClient> _hubContext = hubContext;
    private readonly ILogger<NotificationService> _logger = logger;

    public async Task SendAsync(NotificationType type, int gateId, string title, string body,
        NotificationPriority priority = NotificationPriority.Normal,
        int? sessionId = null, CancellationToken ct = default)
    {
        var gate = await _context.Gates
            .Include(g => g.Client)
            .FirstOrDefaultAsync(g => g.Id == gateId, ct);

        if (gate is null) return;

        var notification = new Notification
        {
            RecipientUserId = gate.Client.UserId,
            GateId = gateId,
            Type = type,
            Priority = priority,
            Title = title,
            Body = body,
            GateSessionId = sessionId,
            IsSent = true,
            SentAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(ct);

        // Push via SignalR
        await _hubContext.Clients
            .Group($"gate-{gateId}")
            .NotificationReceived(new NotificationDto(
                notification.Id,
                type.ToString(),
                priority.ToString(),
                title,
                body,
                notification.CreatedAt,
                gateId));

        _logger.LogInformation("Notification sent: {Type} for Gate {GateId} - {Title}", type, gateId, title);
    }

    public async Task SendSilentAlarmAsync(int gateId, CancellationToken ct = default)
    {
        var gate = await _context.Gates
            .Include(g => g.Client)
            .FirstOrDefaultAsync(g => g.Id == gateId, ct);

        if (gate is null) return;

        var notification = new Notification
        {
            RecipientUserId = gate.Client.UserId,
            GateId = gateId,
            Type = NotificationType.SilentAlarm,
            Priority = NotificationPriority.Critical,
            Title = "⚠️ Silent Alarm",
            Body = $"Silent alarm triggered at {gate.Name}. Someone may be under duress.",
            IsSent = true,
            SentAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(ct);

        await _hubContext.Clients
            .Group($"gate-{gateId}")
            .EmergencyAlert(new EmergencyAlertDto(
                gateId,
                "SilentAlarm",
                "Critical",
                $"Silent alarm triggered at {gate.Name}",
                DateTime.UtcNow));

        _logger.LogWarning("SILENT ALARM triggered at Gate {GateId} ({GateName})", gateId, gate.Name);
    }

    public async Task SendEmergencyAlertAsync(int gateId, EmergencyType emergencyType, string description, CancellationToken ct = default)
    {
        var gate = await _context.Gates
            .Include(g => g.Client)
            .FirstOrDefaultAsync(g => g.Id == gateId, ct);

        if (gate is null) return;

        var notification = new Notification
        {
            RecipientUserId = gate.Client.UserId,
            GateId = gateId,
            Type = NotificationType.Emergency,
            Priority = NotificationPriority.Critical,
            Title = "🚨 Emergency Alert",
            Body = description,
            IsSent = true,
            SentAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync(ct);

        await _hubContext.Clients
            .Group($"gate-{gateId}")
            .EmergencyAlert(new EmergencyAlertDto(
                gateId,
                emergencyType.ToString(),
                "Critical",
                description,
                DateTime.UtcNow));

        _logger.LogCritical("EMERGENCY at Gate {GateId}: {Description}", gateId, description);
    }
}
