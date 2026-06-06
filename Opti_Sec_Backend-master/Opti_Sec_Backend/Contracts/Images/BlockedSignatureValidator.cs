using FluentValidation;


namespace Opti_Sec_Backend.Contracts.Images;

public class BlockedSignatureValidator : AbstractValidator<IFormFile>
{
    public BlockedSignatureValidator()
    {
        RuleFor(x => x)
            .Must((request, context) =>
            {
                // here we check on the first two bytes of the file if contain 
                // on .js or .exe or .msi or not 
                BinaryReader binary = new(request.OpenReadStream());
                // to read the first two binary 
                var bytes = binary.ReadBytes(2);

                var fileSequenceHex = BitConverter.ToString(bytes);
                foreach (var signature in FileSettings.BlockedSignatures)
                {
                    if (signature.Equals(fileSequenceHex, StringComparison.OrdinalIgnoreCase))
                        return false;
                }

                return true;
            })
            .When(x => x is not null)
            .WithMessage("Not allowed file extension");
    }
}
