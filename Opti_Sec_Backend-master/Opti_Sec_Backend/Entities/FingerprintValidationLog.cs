namespace Opti_Sec_Backend.Entities;

public class FingerprintValidationLog
{
    public int Id { get; set; }

    public int GateSessionId { get; set; }
    public GateSession GateSession { get; set; } = default!;

    public int GateId { get; set; }
    public Gate Gate { get; set; } = default!;

    // The member that was expected to be validated, if any.
    // This can be null if the validation was for an unknown person or if the system was
    // not able to determine the expected member.
    public int? ExpectedMemberId { get; set; }
    public Member? ExpectedMember { get; set; }

    public string? FingerprintTemplateHash { get; set; }

    // Whether the validation attempt was successful (i.e., the fingerprint matched the expected member).
    public bool IsMatch { get; set; }

    public int AttemptNumber { get; set; }

    public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

    public string? FailureReason { get; set; }
}
