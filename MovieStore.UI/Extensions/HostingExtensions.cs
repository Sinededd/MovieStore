using MovieStore.UI.Models;
using MovieStore.UI.HelperClasses;
using MovieStore.UI.Services.Authentication;
using MovieStore.UI.Services.FileService;

namespace MovieStore.UI.Extensions;

public static class HostingExtensions
{
    public static void RegisterCustomServices(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<KeycloakData>(builder.Configuration.GetSection("Keycloak"));
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHttpClient<ITokenAccessor, KeycloakTokenAccessor>();
        builder.Services.AddScoped<IFileService, LocalFileService>();

        var uriData = builder.Configuration.GetSection("UriData").Get<UriData>()
            ?? throw new InvalidOperationException("Секция UriData не настроена.");

        if (!Uri.TryCreate(uriData.ApiUri, UriKind.Absolute, out var apiUri))
        {
            throw new InvalidOperationException("UriData:ApiUri должен быть абсолютным URI.");
        }

        builder.Services.AddHttpClient<IMovieService, ApiMovieService>(client =>
            client.BaseAddress = new Uri(apiUri, "movies/"));
        builder.Services.AddHttpClient<IGenreService, ApiGenreService>(client =>
            client.BaseAddress = new Uri(apiUri, "genres/"));
    }
}
