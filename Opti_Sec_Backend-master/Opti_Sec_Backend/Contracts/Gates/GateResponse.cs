namespace Opti_Sec_Backend.Contracts.Gates;

public record GateResponse(
    int Id,
    string Name,
    string? Location,
    string? DeviceId,
    string Status,
    int MaxFailedAttempts,
    string Password,
    string SilentAlarm,
    string ? DeviceApiKey
);

