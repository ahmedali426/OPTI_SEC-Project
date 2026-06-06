namespace Opti_Sec_Backend.Contracts.AI;

public record AITrainingCompleteRequest(
    int MemberId,
    bool Success,
    string? EmbeddingVector,
    string? ErrorMessage
);
