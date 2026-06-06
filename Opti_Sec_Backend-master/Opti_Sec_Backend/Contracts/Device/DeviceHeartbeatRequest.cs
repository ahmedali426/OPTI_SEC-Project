namespace Opti_Sec_Backend.Contracts.Device;

// this to check if the device is alive and update the last seen time of the device in the database
// offline or online 
public record DeviceHeartbeatRequest(
    int GateId,
    string DeviceId,
    DateTime Timestamp
);
