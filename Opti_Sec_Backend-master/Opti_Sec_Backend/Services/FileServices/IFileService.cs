namespace Opti_Sec_Backend.Services.FileServices;

public interface IFileService
{
    Task<string> UploadImageAsync(IFormFile image, CancellationToken cancellationToken = default);

    Task<string> UpdateImageAsync(string? oldImageName, IFormFile newImage, CancellationToken cancellationToken = default);

    Task<bool> DeleteImageAsync(string imageName);
}
