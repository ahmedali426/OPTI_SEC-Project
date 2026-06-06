namespace Opti_Sec_Backend.Contracts.Device;

/// <summary>
/// Represents a single pending command that the IoT device should execute.
/// Returned by the polling endpoint so the device knows exactly what to do.
/// </summary>
public record PendingCommandResponse(
    int CommandId,
    string CommandType,
    string PayloadJson,
    string Source,
    DateTime IssuedAt
);
