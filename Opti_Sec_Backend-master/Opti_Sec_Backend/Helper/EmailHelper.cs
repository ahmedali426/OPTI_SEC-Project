using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Opti_Sec_Backend.Entities;

namespace Opti_Sec_Backend.Helper;

public class EmailHelper(IHttpContextAccessor httpContextAccessor, IEmailSender emailSender)
{
    private readonly IEmailSender _emailSender = emailSender;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task SendConfirmationEmail(ApplicationUser user, string code)
    {
        var emailBody = EmailBodyBuilder.GenerateEmailBody("EmailConfirmation",
            templateModel: new Dictionary<string, string>
            {
            { "{{name}}", $"{user.FName} {user.LName}" },
            { "{{code}}", code }
            }
        );

        BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
            user.Email!,
            "🔐 OPti-Sec App: Email Confirmation",
            emailBody
        ));

        await Task.CompletedTask;
    }


    public async Task SendResetPasswordEmail(ApplicationUser user, string code)
    {
        var emailBody = EmailBodyBuilder.GenerateEmailBody("ForgetPassword",
            templateModel: new Dictionary<string, string>
            {
            { "{{name}}", $"{user.FName} {user.LName}" },
            { "{{code}}", code }
            }
        );

        BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
            user.Email!,
            "🔐 Opti-Sec App: Reset Password",
            emailBody
        ));

        await Task.CompletedTask;
    }

    public async Task CreateUserEmail(string email, string name, string password)
    {
        var emailBody = EmailBodyBuilder.GenerateEmailBody("CreateUser",
            templateModel: new Dictionary<string, string>
            {
                { "{{name}}", name },
                { "{{email}}", email },
                { "{{password}}", password }
            }
        );

        BackgroundJob.Enqueue(() => _emailSender.SendEmailAsync(
            email,
            "🔐 Opti-Sec App: Your Account Credentials",
            emailBody
        ));

        await Task.CompletedTask;
    }


}
