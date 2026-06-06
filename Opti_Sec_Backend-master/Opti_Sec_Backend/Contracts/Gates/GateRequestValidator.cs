using FluentValidation;

namespace Opti_Sec_Backend.Contracts.Gates;

public class GateRequestValidator : AbstractValidator<GateRequest>
{
    public GateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(3, 50);


    }
}
