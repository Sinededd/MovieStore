using MediatR;

namespace MovieStore.API.UseCases;

public sealed record SaveImage(IFormFile File) : IRequest<string>;

public sealed class SaveImageHandler(
    IWebHostEnvironment environment,
    IHttpContextAccessor httpContextAccessor) : IRequestHandler<SaveImage, string>
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    public async Task<string> Handle(SaveImage request, CancellationToken cancellationToken)
    {
        var file = request.File;
        var extension = Path.GetExtension(file.FileName);
        if (file.Length == 0 || file.Length > 5 * 1024 * 1024 ||
            !AllowedExtensions.Contains(extension) ||
            !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Выберите изображение JPG, PNG, WebP или GIF размером до 5 МБ.");
        }

        var requestContext = httpContextAccessor.HttpContext?.Request
            ?? throw new InvalidOperationException("HTTP-запрос недоступен.");
        var imageDirectory = Path.Combine(environment.WebRootPath, "Images");
        Directory.CreateDirectory(imageDirectory);
        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(imageDirectory, fileName);

        await using (var stream = new FileStream(filePath, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return $"{requestContext.Scheme}://{requestContext.Host}{requestContext.PathBase}/Images/{fileName}";
    }
}
