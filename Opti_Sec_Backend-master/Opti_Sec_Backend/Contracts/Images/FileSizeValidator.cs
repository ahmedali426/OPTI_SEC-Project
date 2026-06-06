using FluentValidation;

namespace Opti_Sec_Backend.Contracts.Images;

public class FileSizeValidator : AbstractValidator<IFormFile>
{
    public FileSizeValidator()
    {
        RuleFor(x => x)
            .Must((request, context) => request.Length <= FileSettings.MaxFileSizeInBytes)
            .When(x => x is not null)
            .WithMessage($"Max file size is {FileSettings.MaxFileSizeInMB} MB.");
    }
}
