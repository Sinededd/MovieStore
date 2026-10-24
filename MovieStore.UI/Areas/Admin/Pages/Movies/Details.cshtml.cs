using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MovieStore.UI.Areas.Admin.Pages.Movies;

public class DetailsModel(IMovieService movieService) : PageModel
{
    public Movie Movie { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var response = await movieService.GetMovieByIdAsync(id);
        if (!response.Successfull || response.Data is null) return NotFound();
        Movie = response.Data;
        return Page();
    }
}
