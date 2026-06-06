namespace Opti_Sec_Backend.Errors;

public static class ClientErrors
{
    public static readonly Error InvalidCredentials =
            new("User.InvalidCredentials", "Invalid email/password", StatusCodes.Status400BadRequest);
   
    public static readonly Error NotFound =
            new("Client.NotFound", "Client is Not Found", StatusCodes.Status400BadRequest);
   
    public static readonly Error Unauthorized =
        new("User.Unauthorized", "Unauthorized access", StatusCodes.Status401Unauthorized);

    public static readonly Error UserLockedOut =
    new("User.LockedOut", "User is locked out due to multiple failed login attempts", 403);

    public static readonly Error DisabledUser =
        new("User.IsDisabled", "User Is Disabled ", StatusCodes.Status400BadRequest);

    public static readonly Error DuplicateClientEmail =
       new("Client.EmailIsExist", "the email is exist", StatusCodes.Status400BadRequest);

    public static readonly Error DuplicateUserName =
       new("Client.DuplicateUserName", "Duplicate User Name for another client", StatusCodes.Status400BadRequest);

    public static readonly Error CreateClientFailed =
       new("Client.CreateFailed", "the create client is failed ", StatusCodes.Status400BadRequest);
}
