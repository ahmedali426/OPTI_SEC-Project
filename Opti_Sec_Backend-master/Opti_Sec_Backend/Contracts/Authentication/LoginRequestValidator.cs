using FluentValidation;

namespace Opti_Sec_Backend.Contracts.Authentication;

public class LoginRequestValidator:AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .EmailAddress()
            .NotEmpty();

        RuleFor(x => x.Password).NotEmpty();
    }
}
