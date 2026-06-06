namespace Opti_Sec_Backend.Errors;

public static class AccessLogErrors
{
    public static readonly Error MemberNotBelong =
        new Error("MemberNotBelong", "this member not belong to this client", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidCredentials =
    new(
        "AccessLog.InvalidCredentials",
        "Username and fingerprint are required"
        , StatusCodes.Status400BadRequest
    );
}
