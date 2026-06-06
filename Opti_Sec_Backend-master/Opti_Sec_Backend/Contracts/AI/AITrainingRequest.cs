namespace Opti_Sec_Backend.Contracts.AI;

public record AITrainingRequest(
    int MemberId,
    string Username,
    string ImageUrl,
    DateTime Timestamp
);
