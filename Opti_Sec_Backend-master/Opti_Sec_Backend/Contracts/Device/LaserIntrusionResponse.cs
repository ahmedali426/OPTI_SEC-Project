namespace Opti_Sec_Backend.Contracts.Device;

public record LaserIntrusionResponse(
    bool Received,
    // the id of the emergency event that was triggered by the laser intrusion
    int EmergencyId,
    DeviceCommandsDto Commands
);
