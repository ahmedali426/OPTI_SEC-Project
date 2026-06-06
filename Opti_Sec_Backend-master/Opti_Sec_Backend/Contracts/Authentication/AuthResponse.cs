namespace Opti_Sec_Backend.Contracts.Authentication;

public record AuthResponse(
    string Id,
    string Email,
    string FName,
    string LName,
    string Token,
    int ExpireIn,
    string RefreshToken,
    DateTime RefreshTokenExpiration,
    IEnumerable<string> Roles
);
