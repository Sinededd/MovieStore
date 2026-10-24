namespace MovieStore.API.UseCases;

public static class DeleteImage
{
    public static void IfUploaded(string? imageUrl, IWebHostEnvironment environment)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri) ||
            !uri.AbsolutePath.StartsWith("/Images/", StringComparison.Ordinal))
        {
            return;
        }

        var fileName = Path.GetFileName(uri.AbsolutePath);
        var stem = Path.GetFileNameWithoutExtension(fileName);
        if (stem.Length != 32 || !Guid.TryParseExact(stem, "N", out _))
        {
            return;
        }

        var filePath = Path.Combine(environment.WebRootPath, "Images", fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
