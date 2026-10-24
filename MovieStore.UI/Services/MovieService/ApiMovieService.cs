using System.Text.Json;
using MovieStore.UI.Services.Authentication;

namespace MovieStore.UI.Services.MovieService;

public class ApiMovieService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<ApiMovieService> logger,
    ITokenAccessor tokenAccessor) : IMovieService
{
    private readonly int _pageSize = Math.Max(1, configuration.GetValue("ItemsPerPage", 3));

    public async Task<ResponseData<ListModel<Movie>>> GetMovieListAsync(
        string? genreNormalizedName,
        int pageNo = 1)
    {
        var genrePath = string.IsNullOrWhiteSpace(genreNormalizedName)
            ? string.Empty
            : $"{Uri.EscapeDataString(genreNormalizedName)}/";
        var url = $"{genrePath}?pageNo={Math.Max(1, pageNo)}&pageSize={_pageSize}";

        try
        {
            var response = await httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return Error<ListModel<Movie>>(response.StatusCode, "Данные афиши не получены");
            }

            return await response.Content.ReadFromJsonAsync<ResponseData<ListModel<Movie>>>()
                ?? ResponseData<ListModel<Movie>>.Error("API вернул пустой ответ.");
        }
        catch (Exception exception) when (exception is HttpRequestException or NotSupportedException)
        {
            logger.LogError(exception, "Ошибка обращения к API афиши кинотеатра");
            return ResponseData<ListModel<Movie>>.Error($"Ошибка обращения к API: {exception.Message}");
        }
    }

    public async Task<ResponseData<Movie>> GetMovieByIdAsync(int id)
    {
        try
        {
            var response = await httpClient.GetAsync(id.ToString());
            if (!response.IsSuccessStatusCode)
                return Error<Movie>(response.StatusCode, "Фильм не найден");

            return await response.Content.ReadFromJsonAsync<ResponseData<Movie>>()
                ?? ResponseData<Movie>.Error("API вернул пустой ответ.");
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Фильм {Id} не получен", id);
            return ResponseData<Movie>.Error(exception.Message);
        }
    }

    public async Task UpdateMovieAsync(int id, Movie movie, IFormFile? formFile)
    {
        await tokenAccessor.SetAuthorizationHeaderAsync(httpClient, false);

        using var content = CreateMovieContent(movie, formFile);
        var response = await httpClient.PutAsync(id.ToString(), content);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteMovieAsync(int id)
    {
        await tokenAccessor.SetAuthorizationHeaderAsync(httpClient, false);

        var response = await httpClient.DeleteAsync(id.ToString());
        response.EnsureSuccessStatusCode();
    }

    public async Task<ResponseData<Movie>> CreateMovieAsync(Movie movie, IFormFile? formFile)
    {
        await tokenAccessor.SetAuthorizationHeaderAsync(httpClient, false);
        
        using var content = CreateMovieContent(movie, formFile);
        var response = await httpClient.PostAsync(string.Empty, content);
        if (!response.IsSuccessStatusCode)
        {
            return Error<Movie>(response.StatusCode, "Фильм не добавлен");
        }

        return await response.Content.ReadFromJsonAsync<ResponseData<Movie>>()
            ?? ResponseData<Movie>.Error("API вернул пустой ответ.");
    }

    private static MultipartFormDataContent CreateMovieContent(Movie movie, IFormFile? formFile)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StringContent(JsonSerializer.Serialize(movie)), "movie");
        if (formFile is not null)
        {
            var streamContent = new StreamContent(formFile.OpenReadStream());
            streamContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(formFile.ContentType);
            content.Add(streamContent, "file", Path.GetFileName(formFile.FileName));
        }
        return content;
    }

    private ResponseData<T> Error<T>(System.Net.HttpStatusCode statusCode, string message)
    {
        var fullMessage = $"{message}. HTTP {(int)statusCode}.";
        logger.LogError("{Message}", fullMessage);
        return ResponseData<T>.Error(fullMessage);
    }
}
