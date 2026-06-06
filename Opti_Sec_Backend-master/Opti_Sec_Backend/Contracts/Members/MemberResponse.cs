namespace Opti_Sec_Backend.Contracts.Members;

public record MemberResponse(
    int Id,
    string Name,
    string UserName,
    string Phone,
    string ImageUrl
);
