using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MovieStore.UI.Areas.Admin.Pages.Movies;

public class CreateModel(IMovieService movieService, IGenreService genreService) : PageModel
{
    [BindProperty]
    public Movie Movie { get; set; } = new();

    [BindProperty]
    public IFormFile? Image { get; set; }

    public List<SelectListItem> Genres { get; private set; } = new();

    public async Task OnGetAsync() => await LoadGenresAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var response = await movieService.CreateMovieAsync(Movie, Image);
            if (response.Successfull)
                return RedirectToPage("./Index");
            ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Не удалось создать фильм.");
        }

        await LoadGenresAsync();
        return Page();
    }

    private async Task LoadGenresAsync()
    {
        var response = await genreService.GetGenreListAsync();
        Genres = response.Data?.Select(genre =>
            new SelectListItem(genre.Name, genre.Id.ToString())).ToList() ?? new();
        if (!response.Successfull)
            ModelState.AddModelError(string.Empty, response.ErrorMessage ?? "Не удалось загрузить жанры.");
    }
}
