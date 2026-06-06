using Opti_Sec_Backend.Contracts.Notifications;

namespace Opti_Sec_Backend.Hubs;

public interface IGateHubClient
{
    Task GateStatusChanged(int gateId, string status);
    Task SessionUpdated(GateSessionDto session);
    Task EmergencyAlert(EmergencyAlertDto alert);
    Task NotificationReceived(NotificationDto notification);
    Task CommandAcknowledged(int commandId, string commandType);
    Task BuzzerStatusChanged(int gateId, bool isActive);
}
