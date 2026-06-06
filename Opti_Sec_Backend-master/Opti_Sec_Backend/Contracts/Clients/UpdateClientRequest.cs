namespace Opti_Sec_Backend.Contracts.Clients;

public record UpdateClientRequest(
    string Name,
    string Email,
    string UserName,
    string PhoneNumber,
    IFormFile? Image
);