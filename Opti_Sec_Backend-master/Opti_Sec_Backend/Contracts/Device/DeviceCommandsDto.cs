namespace Opti_Sec_Backend.Contracts.Device;

// this will send to devise to execute the commands, and the device will acknowledge when done
public record DeviceCommandsDto(
    bool OpenGate = false,
    bool ActivateCamera = false,
    bool ActivateBuzzer = false,
    bool StopBuzzer = false,
    bool CaptureFingerprint = false,
    int BuzzerDurationSeconds = 0,
    int DelaySeconds = 0,
    int? ExpectedMemberId = null
);
