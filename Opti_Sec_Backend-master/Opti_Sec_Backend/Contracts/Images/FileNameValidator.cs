using FluentValidation;

namespace Opti_Sec_Backend.Contracts.Images;

public class FileNameValidator : AbstractValidator<IFormFile>
{
    public FileNameValidator()
    {

        RuleFor(x => x.FileName)
            .Matches(@"^[a-zA-Z0-9]+\.[a-zA-Z0-9]+$")
            .When(x => x != null)
            .WithMessage("File name must contain only letters and numbers.");
    }
}
