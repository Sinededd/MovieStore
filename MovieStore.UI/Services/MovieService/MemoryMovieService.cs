namespace MovieStore.UI.Services.MovieService;

public class MemoryMovieService : IMovieService
{
    private readonly List<Movie> _movies = new();
    private readonly List<Genre> _genres;
    private readonly int _itemsPerPage;

    public MemoryMovieService(IConfiguration configuration, IGenreService genreService)
    {
        _itemsPerPage = Math.Max(1, configuration.GetValue("ItemsPerPage", 3));
        _genres = genreService.GetGenreListAsync().GetAwaiter().GetResult().Data ?? new List<Genre>();
        SetupData();
    }

    public Task<ResponseData<ListModel<Movie>>> GetMovieListAsync(string? genreNormalizedName, int pageNo = 1)
    {
        var query = _movies.Where(movie => genreNormalizedName is null ||
            movie.Genre?.NormalizedName == genreNormalizedName);
        var totalItems = query.Count();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)_itemsPerPage));
        var currentPage = Math.Clamp(pageNo, 1, totalPages);
        var model = new ListModel<Movie>
        {
            Items = query.Skip((currentPage - 1) * _itemsPerPage).Take(_itemsPerPage).ToList(),
            CurrentPage = currentPage,
            TotalPages = totalPages
        };

        return Task.FromResult(ResponseData<ListModel<Movie>>.Success(model));
    }

    public Task<ResponseData<Movie>> GetMovieByIdAsync(int id)
    {
        var movie = _movies.FirstOrDefault(item => item.Id == id);
        return Task.FromResult(movie is null
            ? ResponseData<Movie>.Error("Фильм не найден.")
            : ResponseData<Movie>.Success(movie));
    }

    public Task UpdateMovieAsync(int id, Movie movie, IFormFile? formFile)
    {
        var current = _movies.FirstOrDefault(item => item.Id == id);
        if (current is not null)
        {
            current.Name = movie.Name;
            current.Description = movie.Description;
            current.Price = movie.Price;
            current.GenreId = movie.GenreId;
            current.Genre = _genres.FirstOrDefault(genre => genre.Id == movie.GenreId);
            current.Image = movie.Image;
            current.MimeType = movie.MimeType;
        }

        return Task.CompletedTask;
    }

    public Task DeleteMovieAsync(int id)
    {
        _movies.RemoveAll(movie => movie.Id == id);
        return Task.CompletedTask;
    }

    public Task<ResponseData<Movie>> CreateMovieAsync(Movie movie, IFormFile? formFile)
    {
        movie.Id = _movies.Count == 0 ? 1 : _movies.Max(item => item.Id) + 1;
        movie.Genre = _genres.FirstOrDefault(genre => genre.Id == movie.GenreId);
        movie.Image ??= "/images/films/noimage.svg";
        movie.MimeType ??= "image/svg+xml";
        _movies.Add(movie);
        return Task.FromResult(ResponseData<Movie>.Success(movie));
    }

    private void SetupData()
    {
        Add(1, "Дюна: Часть вторая", "Пол Атрейдес объединяется с фрименами и вступает в борьбу за Арракис", 18.00m, "science-fiction", "dune.webp");
        Add(2, "Интерстеллар", "Команда исследователей отправляется сквозь червоточину на поиски нового дома для человечества", 16.50m, "science-fiction", "interstellar.webp");
        Add(3, "Властелин колец: Братство Кольца", "Хоббит Фродо начинает опасное путешествие, чтобы уничтожить Кольцо Всевластия", 15.00m, "fantasy", "ring.webp");
        Add(4, "Гарри Поттер и философский камень", "Юный волшебник впервые попадает в Хогвартс и раскрывает древнюю тайну", 14.90m, "fantasy", "garry.webp");
        Add(5, "Убийство в Восточном экспрессе", "Эркюль Пуаро расследует загадочное убийство в роскошном поезде", 13.40m, "detective", "killexpress.webp");
        Add(6, "Побег из Шоушенка", "История надежды и дружбы двух заключённых в тюрьме Шоушенк", 12.80m, "drama", "showshenk.webp");
        Add(7, "1+1", "Неожиданная дружба аристократа и его нового помощника меняет жизнь обоих", 11.60m, "comedy", "oneplusone.webp");
        Add(8, "Начало", "Специалист по проникновению в сны получает задание внедрить идею в чужое сознание", 17.90m, "thriller", "begin.webp");
        Add(9, "Остров проклятых", "Маршал расследует исчезновение пациентки из клиники на изолированном острове", 14.50m, "thriller", "iland.webp");
    }

    private void Add(int id, string name, string description, decimal price, string genre, string image)
    {
        var entityGenre = _genres.First(item => item.NormalizedName == genre);
        _movies.Add(new Movie
        {
            Id = id,
            Name = name,
            Description = description,
            Price = price,
            Image = $"/images/films/{image}",
            MimeType = "image/webp",
            GenreId = entityGenre.Id,
            Genre = entityGenre
        });
    }
}
