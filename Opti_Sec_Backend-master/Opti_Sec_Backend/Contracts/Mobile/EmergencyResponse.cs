namespace Opti_Sec_Backend.Contracts.Mobile;

// this response will return to the mobile app when an emergency event occurs,
// providing details about the event and its status
public record EmergencyResponse(
    int Id,
    int GateId,
    string GateName,
    // Type can be "Intrusion", "Fire", "Medical", etc.
    string Type,
    // Severity can be "Low", "Medium", "High", "Critical"
    string Severity,
    string Description,
    // Indicates if the buzzer was activated as part of the emergency response
    bool BuzzerActivated,
    DateTime OccurredAt,
    bool IsResolved,
    DateTime? ResolvedAt
);
