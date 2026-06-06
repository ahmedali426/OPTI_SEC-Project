using FluentValidation;

namespace Opti_Sec_Backend.Contracts.Roles;

public class RoleRequestValidator:AbstractValidator<RoleRequest>
{
    public RoleRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(3, 200);
    }
}
