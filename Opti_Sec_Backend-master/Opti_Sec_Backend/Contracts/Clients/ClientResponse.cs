namespace Opti_Sec_Backend.Contracts.Clients;

public record ClientResponse(
    int Id,
    string Name,
    string Email,
    string UserName,
    string PhoneNumber,
    string ImageUrl
);
