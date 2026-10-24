using MediatR;
using Microsoft.EntityFrameworkCore;
using MovieStore.API.Data;
using MovieStore.Domain.Entities;
using MovieStore.Domain.Models;

namespace MovieStore.API.UseCases;

public sealed record GetListOfMovies(
    string? GenreNormalizedName,
    int PageNo = 1,
    int PageSize = 3) : IRequest<ResponseData<ListModel<Movie>>>;

public class GetListOfMoviesHandler(AppDbContext db)
    : IRequestHandler<GetListOfMovies, ResponseData<ListModel<Movie>>>
{
    private const int MaxPageSize = 20;

    public async Task<ResponseData<ListModel<Movie>>> Handle(
        GetListOfMovies request,
        CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var query = db.Movies.AsNoTracking()
            .Include(movie => movie.Genre)
            .Where(movie => request.GenreNormalizedName == null ||
                movie.Genre!.NormalizedName == request.GenreNormalizedName)
            .OrderBy(movie => movie.Id);

        var totalItems = await query.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        var currentPage = Math.Clamp(request.PageNo, 1, totalPages);
        var items = await query
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return ResponseData<ListModel<Movie>>.Success(new ListModel<Movie>
        {
            Items = items,
            CurrentPage = currentPage,
            TotalPages = totalPages
        });
    }
}
