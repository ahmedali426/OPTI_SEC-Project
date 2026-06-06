using System.Security.Cryptography;
using System.Text;
using Hangfire;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Opti_Sec_Backend.Authentications;
using Opti_Sec_Backend.Contracts.Authentication;
using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Helper;
using Opti_Sec_Backend.Persistence;
using ResetPasswordRequest = Opti_Sec_Backend.Contracts.Authentication.ResetPasswordRequest;

namespace Opti_Sec_Backend.Services.AuthServices;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtProvider jwtProvider,
    ILogger<AuthService> logger,
    IEmailSender emailSender,
    IHttpContextAccessor httpContextAccessor,
    EmailHelper emailHelper,
    ApplicationDbContext context) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    private readonly IJwtProvider _jwtProvider = jwtProvider;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly EmailHelper _emailHelper = emailHelper;
    private readonly ApplicationDbContext _context = context;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly int _refreshTokenExpiryDays = 14;

    public async Task<Result<AuthResponse?>> GetTokenAsync(string email, string password, CancellationToken cancellationToken = default)
    {

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return Result.Failure<AuthResponse?>(UserErrors.InvalidCredentials);
        }



        //var isValidPassword = await _userManager.CheckPasswordAsync(user, password);

        //if (!isValidPassword)
        //{
        //    return Result.Failure<AuthResponse?>(UserErrors.InvalidCredentials);
        //}

        var signInResult = await _signInManager.PasswordSignInAsync(user, password, false, true);

        if (!signInResult.Succeeded)
        {
            var error = signInResult.IsLockedOut
                ? UserErrors.UserLockedOut
                : UserErrors.InvalidCredentials;

            return Result.Failure<AuthResponse?>(error);
        }

        var roles = await GetUserRoles(user, cancellationToken);

        var (token, expiresIn) = _jwtProvider.GenerateToken(user,roles);

        var refreshToken = GenerateRefreshToken();

        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = refreshToken,
            ExpireOn = refreshTokenExpiration
        });

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return Result.Failure<AuthResponse?>(UserErrors.UpdateFailed);



        return Result.Success<AuthResponse?>( new AuthResponse(
            user.Id,
            user.Email!,
            user.FName,
            user.LName,
            token,
            expiresIn,
            refreshToken,
            refreshTokenExpiration,
            roles
        ));
    
    }

    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public async Task<Result<AuthResponse?>> GetRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
            return Result.Failure<AuthResponse?>(UserErrors.InvalidJwtToken);

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure<AuthResponse?>(UserErrors.InvalidCredentials);

        if (user.LockoutEnd > DateTime.UtcNow)
            return Result.Failure<AuthResponse?>(UserErrors.UserLockedOut);

        var userRefreshToken = user.RefreshTokens
                                 .FirstOrDefault(x => x.Token == refreshToken && x.IsActive);


        if (userRefreshToken is null)
            return Result.Failure<AuthResponse?>(UserErrors.InvalidRefreshToken);

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        var roles = await GetUserRoles(user,cancellationToken);

        var (newToken, expiresIn) = _jwtProvider.GenerateToken(user,roles);

        var newRefreshToken = GenerateRefreshToken();

        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays);

        user.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshToken,
            ExpireOn = refreshTokenExpiration
        });

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return Result.Failure<AuthResponse?>(UserErrors.UpdateFailed);

        return Result.Success<AuthResponse?>(new AuthResponse(
            user.Id,
            user.Email!,
            user.FName,
            user.LName,
            newToken,
            expiresIn,
            newRefreshToken,
            refreshTokenExpiration,
            roles
        ));

    }
    private async Task<IEnumerable<string>> GetUserRoles(ApplicationUser user, CancellationToken cancellationToken)
    {
        var userRoles = await _userManager.GetRolesAsync(user);

        return (userRoles);
    }
    public async Task<Result> RevokeRefreshTokenAsync(string token, string refreshToken, CancellationToken cancellationToken = default)
    {
        var userId = _jwtProvider.ValidateToken(token);

        if (userId is null)
            return Result.Failure(UserErrors.InvalidJwtToken);

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
            return Result.Failure(UserErrors.InvalidCredentials);

        var userRefreshToken = user.RefreshTokens
                                 .FirstOrDefault(x => x.Token == refreshToken && x.IsActive);



        if (userRefreshToken is null)
            return Result.Failure(UserErrors.InvalidRefreshToken);

        userRefreshToken.RevokedOn = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            return Result.Failure(UserErrors.UpdateFailed);

        return Result.Success();
    }

    public async Task<Result<ForgetPasswordResponse>> ForgetPasswordAsync(string email, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(email);

        // to mislead attackers
        if (user is null)
            return Result.Success<ForgetPasswordResponse>(default);

        if (!user.EmailConfirmed)
            return Result.Failure<ForgetPasswordResponse>(UserErrors.EmailNotConfirmed);

        // Generate 6-digit OTP
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        user.ResetPasswordCode = code;
        user.ResetPasswordCodeExpiration = DateTime.UtcNow.AddMinutes(60);

        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Reset Password OTP: {code}", code);

        await _emailHelper.SendResetPasswordEmail(user, code);

        return Result.Success(new ForgetPasswordResponse(user.Email!));
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !user.EmailConfirmed)
            return Result.Failure(UserErrors.InvalidCode);

        
        if (user.ResetPasswordCode != request.Code ||
            user.ResetPasswordCodeExpiration < DateTime.UtcNow)
        {
            return Result.Failure(UserErrors.InvalidCode);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

        if (!result.Succeeded)
        {
            var error = result.Errors.First();
            return Result.Failure(new Error(error.Code, error.Description, StatusCodes.Status401Unauthorized));
        }

        user.ResetPasswordCode = null;
        user.ResetPasswordCodeExpiration = null;

        await _userManager.UpdateAsync(user);

        return Result.Success();
    }
}
