namespace Opti_Sec_Backend.Errors;

public static class MemberError
{
    public static readonly Error UserNameExist =
           new("UserName.IsExist", "UserName Is exist for another member", StatusCodes.Status400BadRequest);

    public static readonly Error NotFound =
           new("Member.NotFound", "this member not found", StatusCodes.Status400BadRequest);

    public static readonly Error FingerprintAlreadyExists =
    new("Member.FingerprintAlreadyExists", "Fingerprint already assigned to another member", StatusCodes.Status400BadRequest);
}
