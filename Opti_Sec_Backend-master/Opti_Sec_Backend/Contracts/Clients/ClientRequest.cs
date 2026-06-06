namespace Opti_Sec_Backend.Contracts.Clients;

public record ClientRequest(
    string FName,
    string LName,
    string Email,
    string UserName,
    string Password,
    string PhoneNumber,
    IFormFile Image
);
