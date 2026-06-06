namespace Opti_Sec_Backend.Contracts.Device;

public record PasswordValidationRequest(
    int GateId,
    string Password,
    // this will refuse the request if the timestamp is too old (e.g., more than 5 minutes old) to prevent replay attacks
    DateTime Timestamp,
    string DeviceId
);
