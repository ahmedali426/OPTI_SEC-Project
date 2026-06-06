namespace Opti_Sec_Backend.Contracts.Members;

public record MemberUpdateRequest(
    string Name,
    string UserName,
    string Phone,
    IFormFile? Image
);
