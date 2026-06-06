using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Entities;

public class Gate : AuditableEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Location { get; set; } = string.Empty;

    // The hashed password for accessing the gate.
    // This is used for authentication when a user tries to access the gate.
    public string PasswordHash { get; set; } = string.Empty;

    // The hashed value of the silent alarm code.
    // This is used to trigger a silent alarm when a user enters the code at the gate.
    public string SilentAlarmHash { get; set; } = string.Empty;

    // The ID of the physical device associated with this gate, if any.
    public string? DeviceId { get; set; }

    // The API key used to authenticate with the physical device, if any.
    // This is used to ensure that only authorized devices can interact with the gate.
    public string? DeviceApiKey { get; set; }

    // The current status of the gate (e.g., online, offline, maintenance).
    public GateStatus Status { get; set; } = GateStatus.Online;

    public int MaxFailedAttempts { get; set; } = 3;

    public int FailedAttemptsCount { get; set; } = 0;

    public DateTime? LastFailedAttemptAt { get; set; }

    public int ClientId { get; set; }

    public bool IsDeleted { get; set; }

    public Client Client { get; set; } = default!;

    public ICollection<AccessLog> AccessLogs { get; set; } = [];
    public ICollection<GateSession> GateSessions { get; set; } = [];
    public ICollection<PasswordAttempt> PasswordAttempts { get; set; } = [];
    public ICollection<EmergencyEvent> EmergencyEvents { get; set; } = [];
    public ICollection<DeviceCommand> DeviceCommands { get; set; } = [];
    public ICollection<AIValidationLog> AIValidationLogs { get; set; } = [];

    public ICollection<FingerprintValidationLog> FingerprintValidationLogs { get; set; } = [];
}
