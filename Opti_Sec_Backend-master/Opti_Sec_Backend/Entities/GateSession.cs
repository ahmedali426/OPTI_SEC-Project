using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Entities;

// This entity represents an access session at a gate, tracking the entire process
// from initiation to completion, including all validation steps and outcomes.
public class GateSession
{
    public int Id { get; set; }

    // Using a GUID for session token allows for secure, unique identification of each session,
    // which can be used for tracking and correlation across systems without exposing sequential IDs.
    public Guid SessionToken { get; set; } = Guid.CreateVersion7();

    public int GateId { get; set; }
    public Gate Gate { get; set; } = default!;

    public int? MemberId { get; set; }
    public Member? Member { get; set; }

    // The ID of the physical device used during the session, if any. This can help in auditing and troubleshooting,
    public string? DeviceId { get; set; }
    // For AI validation, we may want to store the image or reference to the image captured during the session
    public SessionStatus Status { get; set; } = SessionStatus.PasswordPending;
    // This indicates which step the session is currently on, which can help in resuming or auditing sessions
    public SessionStep CurrentStep { get; set; } = SessionStep.Password;

    public bool IsSilentAlarm { get; set; }

    // Timestamps and results for each validation step, allowing us to track the timing and outcome of each step in the session.
    public DateTime? PasswordValidatedAt { get; set; }
    public bool PasswordPassed { get; set; }

    public DateTime? AIValidatedAt { get; set; }
    public bool AIPassed { get; set; }
    public double? AIConfidenceScore { get; set; }
    public int AIAttemptCount { get; set; }

    public DateTime? FingerprintValidatedAt { get; set; }
    public bool FingerprintPassed { get; set; }
    public int FingerprintAttemptCount { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public SessionResult Result { get; set; } = SessionResult.Pending;

    public string? FailureReason { get; set; }
    public string? CapturedImageName { get; set; }
    public ICollection<AIValidationLog> AIValidationLogs { get; set; } = [];
    public ICollection<FingerprintValidationLog> FingerprintValidationLogs { get; set; } = [];
    public ICollection<DeviceCommand> DeviceCommands { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<PasswordAttempt> PasswordAttempts { get; set; } = [];

    public ICollection<EmergencyEvent> EmergencyEvents { get; set; } = [];
    public AccessLog? AccessLog { get; set; }
}
