using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Contracts.Device;

public record FingerprintVerificationResponse(
    bool Success,
    FingerprintStatus Status,
    bool AccessGranted,
    string? MemberName,
    int AttemptNumber,
    int RemainingAttempts,
    bool Emergency,
    DeviceCommandsDto Commands
);
