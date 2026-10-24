using MovieStore.UI.Models;
using Microsoft.Extensions.Options;
using MovieStore.UI.HelperClasses;
using MovieStore.UI.Services.Authentication;
using MovieStore.UI.Services.FileService;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MovieStore.UI.Controllers;

public class AccountController(
    HttpClient httpClient,
    ITokenAccessor tokenAccessor,
    IOptions<KeycloakData> options,
    IFileService fileService) : Controller
{
    [HttpGet]
    public async Task Login()
    {
        await HttpContext.ChallengeAsync(
            "keycloak",
            new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task LogOut()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignOutAsync(
            "keycloak",
            new AuthenticationProperties
            {
                RedirectUri = Url.Action("Index", "Home")
            });
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterUserViewModel user)
    {
        if (!ModelState.IsValid)
        {
            return View(user);
        }

        try
        {
            await tokenAccessor.SetAuthorizationHeaderAsync(httpClient, true);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty, "Не удалось получить токен приложения.");
            return View(user);
        }

        var avatarUrl = "/images/default-profile-picture.png";

        if (user.Avatar is not null)
        {
            try
            {
                avatarUrl = await fileService.SaveFileAsync(user.Avatar);
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(nameof(user.Avatar), exception.Message);
                return View(user);
            }
        }

        var newUser = new CreateUserModel
        {
            Username = user.Email,
            Email = user.Email
        };

        newUser.Attributes.Add("avatar", avatarUrl);
        newUser.Credentials.Add(
            new UserCredentials { Value = user.Password });

        var settings = options.Value;
        var requestUri =
            $"{settings.Host}/admin/realms/{settings.Realm}/users";

        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(newUser, serializerOptions);

        using var content =
            new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await httpClient.PostAsync(requestUri, content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError(
            string.Empty,
            $"Не удалось зарегистрировать пользователя. HTTP {(int)response.StatusCode}.");

        return View(user);
    }
}

internal class CreateUserModel
{
    public Dictionary<string, string> Attributes { get; set; } = new();

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;
    public bool EmailVerified { get; set; } = true;

    public List<UserCredentials> Credentials { get; set; } = new();
}

internal class UserCredentials
{
    public string Type { get; set; } = "password";
    public bool Temporary { get; set; } = false;
    public string Value { get; set; } = string.Empty;
}