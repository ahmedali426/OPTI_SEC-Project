using Microsoft.AspNetCore.Identity;

namespace Opti_Sec_Backend.Entities;

public class ApplicationUser: IdentityUser
{
    public ApplicationUser()
    {
        Id = Guid.CreateVersion7().ToString();
        SecurityStamp = Guid.CreateVersion7().ToString();
    }
    public string FName { get; set; } = string.Empty;
    public string LName { get; set; } = string.Empty;

    public Client? client { get; set; }
    public string? EmailConfirmationCode { get; set; }
    public DateTime? EmailConfirmationCodeExpiration { get; set; }
    public string? ResetPasswordCode { get; set; }
    public DateTime? ResetPasswordCodeExpiration { get; set; }

    // For FCM token management, we can store the latest token for each user. In a real-world application, you might want to
    // allow multiple tokens per user (e.g., for multiple devices), but for simplicity, we'll store just one here.
    // firbase cloud messaging token for push notifications 
    public string? FcmToken { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = [];
}
