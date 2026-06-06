using Opti_Sec_Backend.Enums;

namespace Opti_Sec_Backend.Entities;

public class EmergencyEvent
{
    public int Id { get; set; }

    public int GateId { get; set; }
    public Gate Gate { get; set; } = default!;
    // open gate , activate buzzer, stop buzzer, capture image, capture fingerprint
    public EmergencyType Type { get; set; }

    // low, medium, high
    public EmergencySeverity Severity { get; set; }

    // description of the event, e.g., "Unauthorized access attempt detected at Gate 3"
    public string Description { get; set; } = string.Empty;
    // whether to open the gate or not
    public bool BuzzerActivated { get; set; }
    public int BuzzerDurationSeconds { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }

    public bool IsResolved { get; set; }

    public string? ResolvedByUserId { get; set; }
    public ApplicationUser? ResolvedBy { get; set; }

    public int? GateSessionId { get; set; }
    public GateSession? GateSession { get; set; }

    // optional URL to an image captured during the emergency event (e.g., from a security camera)
    public string? ImageUrl { get; set; }
}
