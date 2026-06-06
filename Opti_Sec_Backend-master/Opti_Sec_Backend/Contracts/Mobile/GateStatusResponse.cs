namespace Opti_Sec_Backend.Contracts.Mobile;

// This record represents the response for the gate status endpoint, which provides information
// about the current status of a gate, including its name, status, session token (if online)
// , buzzer status, active emergencies, and last heartbeat time.
public record GateStatusResponse(
    int GateId,
    string Name,
    // if the gate is online, offline , or Maintenance 
    string Status,
    // if the gate is online, the session token for the current session, otherwise null
    string? CurrentSessionToken,
    // if the gate is online, the time of the last successful heartbeat, otherwise null
    bool BuzzerActive,
    // if the gate is online, the number of active emergencies
    int ActiveEmergencies,
    // if the gate is online, the time of the last successful heartbeat, otherwise null
    DateTime? LastHeartbeat
);
