namespace Opti_Sec_Backend.Contracts.Members;

public record MemberRequest(
    string FName,
    string LName,
    string UserName,
    string Phone,
    IFormFile Image
);
