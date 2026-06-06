using Opti_Sec_Backend.Abstractions;

namespace Opti_Sec_Backend.Errors;

public static class UserErrors
{
    public static readonly Error InvalidCredentials =
            new("User.InvalidCredentials", "Invalid email/password", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidJwtToken =
        new("User.InvalidJwtToken", "Invalid Jwt token", StatusCodes.Status400BadRequest);

    public static readonly Error InvalidRefreshToken =
        new("User.InvalidRefreshToken", "Invalid refresh token", StatusCodes.Status400BadRequest);

    public static readonly Error UpdateFailed =
    new("User.UpdateFailed", "Failed to update user", StatusCodes.Status400BadRequest);

    public static readonly Error Unauthorized =
        new("User.Unauthorized", "Unauthorized access", StatusCodes.Status401Unauthorized);

    public static readonly Error UserLockedOut =
    new("User.LockedOut", "User is locked out due to multiple failed login attempts", 403);

    public static readonly Error DisabledUser =
        new("User.IsDisabled","User Is Disabled ",StatusCodes.Status400BadRequest);

    public static readonly Error EmailNotConfirmed =
        new("User.EmailNotConfirmed", "Email is not confirmed", StatusCodes.Status401Unauthorized);

    public static readonly Error InvalidCode =
       new("User.InvalidCode", "Invalid code", StatusCodes.Status401Unauthorized);
}
