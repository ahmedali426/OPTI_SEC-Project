namespace Opti_Sec_Backend.Contracts.Gates;

public record GateRequest(
    string Name,
    string? Location,
    string DeviceId,
    string Password,
    string SilentAlarm
);
