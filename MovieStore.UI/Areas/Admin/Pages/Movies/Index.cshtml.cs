using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MovieStore.UI.Areas.Admin.Pages.Movies;

public class IndexModel(IMovieService movieService) : PageModel
{
    public ListModel<Movie> Movies { get; private set; } = new();
    public string? ErrorMessage { get; private set; }

    public async Task OnGetAsync(int pageNo = 1)
    {
        var response = await movieService.GetMovieListAsync(null, pageNo);
        if (response.Successfull && response.Data is not null)
            Movies = response.Data;
        else
            ErrorMessage = response.ErrorMessage ?? "Не удалось загрузить фильмы.";
    }
}
