namespace MovieStore.UI.Services.GenreService;

public class MemoryGenreService : IGenreService
{
    public Task<ResponseData<List<Genre>>> GetGenreListAsync()
    {
        var genres = new List<Genre>
        {
            new() { Id = 1, Name = "Фантастика", NormalizedName = "science-fiction" },
            new() { Id = 2, Name = "Фэнтези", NormalizedName = "fantasy" },
            new() { Id = 3, Name = "Детектив", NormalizedName = "detective" },
            new() { Id = 4, Name = "Драма", NormalizedName = "drama" },
            new() { Id = 5, Name = "Комедия", NormalizedName = "comedy" },
            new() { Id = 6, Name = "Триллер", NormalizedName = "thriller" }
        };

        return Task.FromResult(ResponseData<List<Genre>>.Success(genres));
    }
}
