using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Services.NotificationServices;

public interface INotificationService
{
    Task SendAsync(NotificationType type, int gateId, string title, string body,
        NotificationPriority priority = NotificationPriority.Normal,
        int? sessionId = null, CancellationToken ct = default);

    Task SendSilentAlarmAsync(int gateId, CancellationToken ct = default);
    Task SendEmergencyAlertAsync(int gateId, EmergencyType emergencyType, string description, CancellationToken ct = default);
}
