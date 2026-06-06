namespace Opti_Sec_Backend.Contracts.Notifications;

// this will send when an emergency alert is triggered, such as a fire alarm or security breach.
// It will contain information about the type of alert, its severity, and when it occurred.
public record EmergencyAlertDto(
    int GateId,
    string Type,
    string Severity,
    string Description,
    DateTime OccurredAt
);
