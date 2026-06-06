namespace Opti_Sec_Backend.Contracts.Clients;

public record GetAllClientResponseForAI(
    int Id,
    string Name,
    string UserName,
    string ImageUrl,
    bool IsDeleted
);
