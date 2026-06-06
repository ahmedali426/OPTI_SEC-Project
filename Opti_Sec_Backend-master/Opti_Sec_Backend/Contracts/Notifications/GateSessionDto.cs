namespace Opti_Sec_Backend.Contracts.Notifications;

public record GateSessionDto(
    int Id,
    Guid SessionToken,
    int GateId,
    string Status,
    string CurrentStep,
    string? MemberName,
    bool IsSilentAlarm,
    DateTime StartedAt
);
