using FluentValidation;

namespace Opti_Sec_Backend.Contracts.Members;

public class SetFingerPrintRequestValidator : AbstractValidator<SetFingerPrintRequest>
{
    public SetFingerPrintRequestValidator()
    {
        RuleFor(x => x.MemberId)
            .GreaterThan(0)
            .NotEmpty();

        RuleFor(x => x.FingerprintTemplate)
            .NotEmpty();
    }
}
