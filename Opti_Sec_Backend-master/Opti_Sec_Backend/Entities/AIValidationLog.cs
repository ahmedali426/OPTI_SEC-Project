namespace Opti_Sec_Backend.Entities;

public class AIValidationLog
{
    public int Id { get; set; }

    public int GateSessionId { get; set; }
    public GateSession GateSession { get; set; } = default!;

    public int GateId { get; set; }
    public Gate Gate { get; set; } = default!;

    public string? ImageUrl { get; set; }

    public bool IsAuthorized { get; set; }
    public double ConfidenceScore { get; set; }

    public int? MatchedMemberId { get; set; }
    public Member? MatchedMember { get; set; }

    public int AttemptNumber { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }

    // response time in milliseconds from ai request to response
    public int ResponseTimeMs { get; set; }

    // Store the raw JSON response from the AI service for debugging and analysis
    public string? AIRawResponseJson { get; set; }
    // Store any error message if the AI request failed
    public string? ErrorMessage { get; set; }
}
