namespace MovieStore.UI.Services.FileService;

public class LocalFileService(IWebHostEnvironment environment)
    : IFileService
{
    public async Task<string> SaveFileAsync(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (extension is not (".jpg" or ".jpeg" or ".png" or ".webp"))
        {
            throw new ArgumentException(
                "Допустимы изображения JPG, PNG и WebP.");
        }

        if (file.Length == 0 || file.Length > 2 * 1024 * 1024)
        {
            throw new ArgumentException(
                "Файл должен быть непустым и размером не более 2 МБ.");
        }

        var fileName = $"{Guid.NewGuid():N}{extension}";

        var directory = Path.Combine(environment.WebRootPath, "Images");
        Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, fileName);

        await using var stream = new FileStream(filePath, FileMode.CreateNew);
        await file.CopyToAsync(stream);

        return $"/Images/{fileName}";
    }
}