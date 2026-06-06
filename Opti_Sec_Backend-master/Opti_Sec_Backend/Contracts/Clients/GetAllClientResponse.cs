namespace Opti_Sec_Backend.Contracts.Clients;

public record GetAllClientResponse(
    int Id,
    string Name,
    string ImageUrl
);
