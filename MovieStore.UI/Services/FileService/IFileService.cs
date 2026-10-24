namespace MovieStore.UI.Services.FileService;

public interface IFileService
{
    Task<string> SaveFileAsync(IFormFile file);
}