namespace Opti_Sec_Backend.Services.FileServices;

public class FileService : IFileService
{
    private readonly string _imagesPath;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public FileService(IWebHostEnvironment webHostEnvironment, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;

        _imagesPath = Path.Combine(webHostEnvironment.WebRootPath, "Images");

        if (!Directory.Exists(_imagesPath))
            Directory.CreateDirectory(_imagesPath);
    }

    public async Task<string> UploadImageAsync(IFormFile image, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(image.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";

        var path = Path.Combine(_imagesPath, fileName);

        using var stream = File.Create(path);
        await image.CopyToAsync(stream, cancellationToken);

        return fileName;

        //var extension = Path.GetExtension(image.FileName);
        //var fileName = $"{Guid.NewGuid()}{extension}";

        //var path = Path.Combine(_imagesPath, fileName);

        //using var stream = File.Create(path);
        //await image.CopyToAsync(stream, cancellationToken);

        //var request = _httpContextAccessor.HttpContext!.Request;
        //var baseUrl = $"{request.Scheme}://{request.Host}";

        //return $"{baseUrl}/Images/{fileName}";
    }

    public Task<bool> DeleteImageAsync(string imageName)
    {
        var path = Path.Combine(_imagesPath, imageName);

        if (!File.Exists(path))
            return Task.FromResult(false);

        File.Delete(path);

        return Task.FromResult(true);
    }

    public async Task<string> UpdateImageAsync(string? oldImageName, IFormFile newImage, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(oldImageName))
        {
            await DeleteImageAsync(oldImageName);
        }

        return await UploadImageAsync(newImage, cancellationToken);
    }
}