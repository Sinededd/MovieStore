using System.Net.Http.Json;

namespace MovieStore.UI.Services.GenreService;

public class ApiGenreService(HttpClient httpClient, ILogger<ApiGenreService> logger) : IGenreService
{
    public async Task<ResponseData<List<Genre>>> GetGenreListAsync()
    {
        try
        {
            var response = await httpClient.GetAsync(string.Empty);
            if (!response.IsSuccessStatusCode)
            {
                var message = $"Жанры не получены. HTTP {(int)response.StatusCode}.";
                logger.LogError("{Message}", message);
                return ResponseData<List<Genre>>.Error(message);
            }

            return await response.Content.ReadFromJsonAsync<ResponseData<List<Genre>>>()
                ?? ResponseData<List<Genre>>.Error("API вернул пустой ответ.");
        }
        catch (Exception exception) when (exception is HttpRequestException or NotSupportedException)
        {
            logger.LogError(exception, "Ошибка обращения к API жанров");
            return ResponseData<List<Genre>>.Error($"Ошибка обращения к API: {exception.Message}");
        }
    }
}
