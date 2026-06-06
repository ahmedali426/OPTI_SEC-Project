namespace Opti_Sec_Backend.Contracts.Device;

public record PasswordValidationResponse(
    bool Success,
    Guid? SessionToken,
    // PasswordStatus can be "Valid", "Invalid", "LockedOut", "Expired", etc.
    string PasswordStatus,
    string? NextStep,
    int AttemptNumber,
    int RemainingAttempts,
    bool Emergency,
    DeviceCommandsDto Commands
);
