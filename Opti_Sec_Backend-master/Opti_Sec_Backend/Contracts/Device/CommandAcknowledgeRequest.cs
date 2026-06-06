namespace Opti_Sec_Backend.Contracts.Device;

// when the device receives a command, it should acknowledge that it has received
// the command by sending this request to the backend. The backend can then update
// the command status to "acknowledged" and proceed with further processing if needed.
public record CommandAcknowledgeRequest(
    // device command id 
    int CommandId,
    string DeviceId
);
