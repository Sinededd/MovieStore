using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MovieStore.UI.Areas.Admin.Pages.Movies;

public class DeleteModel(IMovieService movieService) : PageModel
{
    public Movie Movie { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var response = await movieService.GetMovieByIdAsync(id);
        if (!response.Successfull || response.Data is null) return NotFound();
        Movie = response.Data;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await movieService.DeleteMovieAsync(id);
            return RedirectToPage("./Index");
        }
        catch (HttpRequestException exception)
        {
            ModelState.AddModelError(string.Empty, $"Не удалось удалить фильм: {exception.Message}");
            return await OnGetAsync(id);
        }
    }
}
