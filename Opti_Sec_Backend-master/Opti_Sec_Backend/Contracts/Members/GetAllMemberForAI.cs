namespace Opti_Sec_Backend.Contracts.Members;

public record GetAllMemberForAI(
    int Id,
    string Name,
    string UserName,
    string ImageUrl,
    bool IsDeleted
);