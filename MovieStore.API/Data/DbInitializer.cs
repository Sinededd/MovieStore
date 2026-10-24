using Microsoft.EntityFrameworkCore;
using MovieStore.Domain.Entities;

namespace MovieStore.API.Data;

public static class DbInitializer
{
    public static async Task SeedData(WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        var baseUrl = app.Configuration["ApiBaseUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("ApiBaseUrl не настроен.");

        if (await context.Genres.AnyAsync())
        {
            var movies = await context.Movies.ToListAsync();
            var posterNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["bruschetta.svg"] = "dune.webp",
                ["caesar.svg"] = "interstellar.webp",
                ["kharcho.svg"] = "ring.webp",
                ["borscht.svg"] = "garry.webp",
                ["carbonara.svg"] = "killexpress.webp",
                ["steak.svg"] = "showshenk.webp",
                ["lemonade.svg"] = "oneplusone.webp",
                ["coffee.svg"] = "begin.webp",
                ["cheesecake.svg"] = "iland.webp"
            };

            foreach (var movie in movies)
            {
                if (Uri.TryCreate(movie.Image, UriKind.Absolute, out var uploadedImage) &&
                    uploadedImage.AbsolutePath.StartsWith("/Images/", StringComparison.Ordinal))
                {
                    continue;
                }

                var fileName = string.IsNullOrWhiteSpace(movie.Image)
                    ? "noimage.svg"
                    : Path.GetFileName(movie.Image);
                if (posterNames.TryGetValue(fileName, out var posterName))
                {
                    fileName = posterName;
                }

                movie.Image = $"{baseUrl}/images/{fileName}";
                movie.MimeType = fileName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                    ? "image/svg+xml"
                    : "image/webp";
            }

            await context.SaveChangesAsync();
            return;
        }

        var genres = new List<Genre>
        {
            new() { Name = "Фантастика", NormalizedName = "science-fiction" },
            new() { Name = "Фэнтези", NormalizedName = "fantasy" },
            new() { Name = "Детектив", NormalizedName = "detective" },
            new() { Name = "Драма", NormalizedName = "drama" },
            new() { Name = "Комедия", NormalizedName = "comedy" },
            new() { Name = "Триллер", NormalizedName = "thriller" }
        };

        context.Genres.AddRange(genres);
        await context.SaveChangesAsync();

        Genre Find(string normalizedName) =>
            genres.First(genre => genre.NormalizedName == normalizedName);
        string Image(string fileName) => $"{baseUrl}/images/{fileName}";

        context.Movies.AddRange(
            CreateMovie("Дюна: Часть вторая", "Пол Атрейдес объединяется с фрименами и вступает в борьбу за Арракис", 18.00m, Find("science-fiction"), Image("dune.webp")),
            CreateMovie("Интерстеллар", "Команда исследователей отправляется сквозь червоточину на поиски нового дома для человечества", 16.50m, Find("science-fiction"), Image("interstellar.webp")),
            CreateMovie("Властелин колец: Братство Кольца", "Хоббит Фродо начинает опасное путешествие, чтобы уничтожить Кольцо Всевластия", 15.00m, Find("fantasy"), Image("ring.webp")),
            CreateMovie("Гарри Поттер и философский камень", "Юный волшебник впервые попадает в Хогвартс и раскрывает древнюю тайну", 14.90m, Find("fantasy"), Image("garry.webp")),
            CreateMovie("Убийство в Восточном экспрессе", "Эркюль Пуаро расследует загадочное убийство в роскошном поезде", 13.40m, Find("detective"), Image("killexpress.webp")),
            CreateMovie("Побег из Шоушенка", "История надежды и дружбы двух заключённых в тюрьме Шоушенк", 12.80m, Find("drama"), Image("showshenk.webp")),
            CreateMovie("1+1", "Неожиданная дружба аристократа и его нового помощника меняет жизнь обоих", 11.60m, Find("comedy"), Image("oneplusone.webp")),
            CreateMovie("Начало", "Специалист по проникновению в сны получает задание внедрить идею в чужое сознание", 17.90m, Find("thriller"), Image("begin.webp")),
            CreateMovie("Остров проклятых", "Маршал расследует исчезновение пациентки из клиники на изолированном острове", 14.50m, Find("thriller"), Image("iland.webp")));

        await context.SaveChangesAsync();
    }

    private static Movie CreateMovie(string name, string description, decimal price, Genre genre, string image) => new()
    {
        Name = name,
        Description = description,
        Price = price,
        Genre = genre,
        GenreId = genre.Id,
        Image = image,
        MimeType = image.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
            ? "image/svg+xml"
            : "image/webp"
    };
}
