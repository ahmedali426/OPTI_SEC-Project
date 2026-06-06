namespace Opti_Sec_Backend.Contracts.Device;

public record FingerprintVerificationRequest(
    Guid SessionToken,
    int MemberId,
    string FingerprintTemplate,
    string DeviceId
);
