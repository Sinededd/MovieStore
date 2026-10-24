namespace MovieStore.UI.Services.GenreService;

public interface IGenreService
{
    Task<ResponseData<List<Genre>>> GetGenreListAsync();
}
