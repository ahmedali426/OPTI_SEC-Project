using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Entities;

public class PasswordAttempt
{
    public int Id { get; set; }

    public int GateId { get; set; }
    public Gate Gate { get; set; } = default!;

    public int? MemberId { get; set; }
    public Member? Member { get; set; }

    // Optional: If we want to track which user made the attempt (if authenticated), we can include this
    public string? DeviceId { get; set; }

    public string PasswordHashAttempt { get; set; } = string.Empty;

    // The result of the password attempt (Correct, SilentAlarm, Wrong)
    public PasswordStatus Status { get; set; }

    public int AttemptNumber { get; set; }

    public bool TriggeredEmergency { get; set; }

    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    public int? GateSessionId { get; set; }
    public GateSession? GateSession { get; set; }
    // Optional: If we want to log the IP address of the attempt (if applicable)
    public string? IpAddress { get; set; }
}
