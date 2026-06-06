using Microsoft.AspNetCore.Identity.Data;
using Opti_Sec_Backend.Contracts.Authentication;

namespace Opti_Sec_Backend.Services.AuthServices;

public interface IAuthService
{
    Task<Result<AuthResponse?>> GetTokenAsync(string email, string password,
        CancellationToken cancellationToken = default);

    Task<Result<AuthResponse?>> GetRefreshTokenAsync(string token, string refreshToken
        ,CancellationToken cancellationToken = default);

    Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken,
        CancellationToken cancellationToken = default);

    Task<Result<ForgetPasswordResponse>> ForgetPasswordAsync(string email, CancellationToken cancellationToken);

    Task<Result> ResetPasswordAsync(Contracts.Authentication.ResetPasswordRequest request, CancellationToken cancellationToken);

}
