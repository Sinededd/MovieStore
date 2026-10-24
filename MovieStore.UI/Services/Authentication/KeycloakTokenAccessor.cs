using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using MovieStore.UI.HelperClasses;
using System.Text.Json;

namespace MovieStore.UI.Services.Authentication;

internal class KeycloakTokenAccessor(
    IHttpContextAccessor contextAccessor,
    IOptions<KeycloakData> options,
    HttpClient httpClient) : ITokenAccessor
{
    public async Task SetAuthorizationHeaderAsync(
        HttpClient targetClient,
        bool isClient)
    {
        var token = isClient
            ? await GetClientToken()
            : await GetUserToken();

        targetClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<string> GetUserToken()
    {
        var context = contextAccessor.HttpContext
            ?? throw new AuthenticationFailureException(
                "Отсутствует HTTP-контекст");

        var session = await context.AuthenticateAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        if (!session.Succeeded)
        {
            throw new AuthenticationFailureException(
                "Пользователь не аутентифицирован");
        }

        return await context.GetTokenAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            "access_token")
            ?? throw new AuthenticationFailureException(
                "В сессии отсутствует access_token");
    }

    private async Task<string> GetClientToken()
    {
        var settings = options.Value;

        var requestUri =
            $"{settings.Host}/realms/{settings.Realm}/protocol/openid-connect/token";

        using var content = new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["client_id"] = settings.ClientId,
                ["client_secret"] = settings.ClientSecret,
                ["grant_type"] = "client_credentials"
            });

        using var response = await httpClient.PostAsync(requestUri, content);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        return document.RootElement
            .GetProperty("access_token")
            .GetString()
            ?? throw new AuthenticationFailureException(
                "Keycloak не вернул access_token приложения");
    }
}