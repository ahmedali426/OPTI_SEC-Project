namespace Opti_Sec_Backend.Errors;

public static class EmergencyErrors
{
    public static readonly Error NotFound =
        new("Emergency.NotFound", "Emergency event not found", StatusCodes.Status404NotFound);

    public static readonly Error AlreadyResolved =
        new("Emergency.AlreadyResolved", "Emergency has already been resolved", StatusCodes.Status409Conflict);
}
