namespace Opti_Sec_Backend.Contracts.AI;

public record AIRecognitionResultRequest(
    Guid SessionToken,
    bool IsAuthorized,
    double ConfidenceScore,
    int? MatchedMemberId,
    int ProcessingTimeMs,
    IFormFile? ImageUrl
);
