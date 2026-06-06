namespace Opti_Sec_Backend.Contracts.Device;

public record LaserIntrusionRequest(
    int GateId,
    string DeviceId,
    DateTime Timestamp
);
