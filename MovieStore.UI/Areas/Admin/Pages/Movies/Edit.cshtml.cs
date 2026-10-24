using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MovieStore.UI.Areas.Admin.Pages.Movies;

public class EditModel(IMovieService movieService, IGenreService genreService) : PageModel
{
    [BindProperty]
    public Movie Movie { get; set; } = new();

    [BindProperty]
    public IFormFile? Image { get; set; }

    public List<SelectListItem> Genres { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var response = await movieService.GetMovieByIdAsync(id);
        if (!response.Successfull || response.Data is null) return NotFound();
        Movie = response.Data;
        await LoadGenresAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (id != Movie.Id) return BadRequest();
        if (ModelState.IsValid)
        {
            try
            {
                await movieService.UpdateMovieAsync(id, Movie, Image);
                return RedirectToPage("./Index");
            }
            catch (HttpRequestException exception)
            {
                ModelState.AddModelError(string.Empty, $"Не удалось изменить фильм: {exception.Message}");
            }
        }

        var current = await movieService.GetMovieByIdAsync(id);
        Movie.Image = current.Data?.Image;
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
