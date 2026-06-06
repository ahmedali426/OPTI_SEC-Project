namespace Opti_Sec_Backend.Contracts.Authentication;

public record LoginRequest(
    string Email,
    string Password
);
