namespace MovieStore.UI.Controllers;

public class MovieController(IMovieService movieService, IGenreService genreService) : Controller
{
    public async Task<IActionResult> Index(string? genre, int pageNo = 1)
    {
        var genresResponse = await genreService.GetGenreListAsync();
        if (!genresResponse.Successfull || genresResponse.Data is null)
        {
            return NotFound(genresResponse.ErrorMessage);
        }

        var moviesResponse = await movieService.GetMovieListAsync(genre, pageNo);
        if (!moviesResponse.Successfull || moviesResponse.Data is null)
        {
            return NotFound(moviesResponse.ErrorMessage);
        }

        ViewData["Genres"] = genresResponse.Data;
        ViewData["CurrentGenre"] = genresResponse.Data
            .FirstOrDefault(item => item.NormalizedName == genre)?.Name ?? "Все жанры";

        return View(moviesResponse.Data);
    }
}
