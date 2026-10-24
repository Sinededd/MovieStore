namespace MovieStore.UI.Services.MovieService;

public interface IMovieService
{
    Task<ResponseData<ListModel<Movie>>> GetMovieListAsync(string? genreNormalizedName, int pageNo = 1);
    Task<ResponseData<Movie>> GetMovieByIdAsync(int id);
    Task UpdateMovieAsync(int id, Movie movie, IFormFile? formFile);
    Task DeleteMovieAsync(int id);
    Task<ResponseData<Movie>> CreateMovieAsync(Movie movie, IFormFile? formFile);
}
