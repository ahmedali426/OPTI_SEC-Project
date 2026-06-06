namespace Opti_Sec_Backend.Errors;

public static class SessionErrors
{
    public static readonly Error NotFound =
        new("Session.NotFound", "Gate session not found", StatusCodes.Status404NotFound);

    public static readonly Error Expired =
        new("Session.Expired", "Gate session has expired", StatusCodes.Status410Gone);

    public static readonly Error InvalidStep =
        new("Session.InvalidStep", "Invalid session step for this operation", StatusCodes.Status400BadRequest);

    public static readonly Error AlreadyCompleted =
        new("Session.AlreadyCompleted", "Gate session has already been completed", StatusCodes.Status409Conflict);
}
