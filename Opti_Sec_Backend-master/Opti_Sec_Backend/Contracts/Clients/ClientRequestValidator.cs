using FluentValidation;
using Opti_Sec_Backend.Contracts.Images;

namespace Opti_Sec_Backend.Contracts.Clients;

public class ClientRequestValidator : AbstractValidator<ClientRequest>
{
    public ClientRequestValidator()
    {
        RuleFor(x => x.FName)
            .NotEmpty()
            .Length(3, 50);

        RuleFor(x => x.LName)
            .NotEmpty()
            .Length(3, 50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.UserName)
            .NotEmpty()
            .Length(3, 30)
            .Matches(@"^[a-zA-Z0-9._]+$")
            .WithMessage("Username can only contain letters, numbers, dot and underscore.");

        RuleFor(x => x.Password)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\@\!\?\*\.\-_]).{8,}$")
            .WithMessage("Password must be at least 8 characters and include uppercase, lowercase, number, and special character.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches(@"^(010|011|012|015)\d{8}$")
            .WithMessage("Invalid Egyptian phone number.");

        RuleFor(x => x.Image)
            .NotNull()
            .SetValidator(new FileSizeValidator())
            .SetValidator(new FileNameValidator())
            .SetValidator(new BlockedSignatureValidator());

        RuleFor(x => x.Image.FileName)
            .Must(fileName =>
            {
                var extension = Path.GetExtension(fileName).ToLower();
                return FileSettings.AllowedImagesExtensions.Contains(extension);
            })
            .When(x => x.Image != null)
            .WithMessage("Only .jpg, .jpeg, .png files are allowed.");

    }
}
