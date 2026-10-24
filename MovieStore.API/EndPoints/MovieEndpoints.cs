using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using MovieStore.API.Data;
using MovieStore.API.UseCases;
using MovieStore.Domain.Entities;
using MovieStore.Domain.Models;

namespace MovieStore.API.EndPoints;

public static class MovieEndpoints
{
    public static void MapMovieEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/movies")
                        .WithTags("Movies")
                        .DisableAntiforgery()
                        .RequireAuthorization("admin");

        group.MapGet("/", GetAllMovies).WithName("GetAllMovies").AllowAnonymous();;
        group.MapGet("/{genre:regex(^[a-zA-Z-]+$)}", GetAllMovies).WithName("GetMoviesByGenre").AllowAnonymous();;

        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var movie = await db.Movies.AsNoTracking().Include(item => item.Genre)
                .FirstOrDefaultAsync(item => item.Id == id);
            return movie is null
                ? Results.NotFound(ResponseData<Movie>.Error("Фильм не найден."))
                : Results.Ok(ResponseData<Movie>.Success(movie));
        }).WithName("GetMovieById").AllowAnonymous();

        group.MapPost("/", async (
            [FromForm] string movie,
            [FromForm] IFormFile? file,
            AppDbContext db,
            IMediator mediator,
            HttpContext context,
            IWebHostEnvironment environment) =>
        {
            Movie? newMovie;
            try { newMovie = JsonSerializer.Deserialize<Movie>(movie); }
            catch (JsonException) { return Results.BadRequest(ResponseData<Movie>.Error("Некорректные данные фильма.")); }

            if (newMovie is null || string.IsNullOrWhiteSpace(newMovie.Name) ||
                string.IsNullOrWhiteSpace(newMovie.Description) || newMovie.Price <= 0)
            {
                return Results.BadRequest(ResponseData<Movie>.Error(
                    "Название, описание и цена билета должны быть заполнены."));
            }

            if (!await db.Genres.AnyAsync(genre => genre.Id == newMovie.GenreId))
            {
                return Results.BadRequest(ResponseData<Movie>.Error("Указанный жанр не существует."));
            }

            newMovie.Id = 0;
            newMovie.Genre = null;
            newMovie.Image = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.PathBase}/images/noimage.svg";
            newMovie.MimeType = "image/svg+xml";
            if (file is not null)
            {
                try { newMovie.Image = await mediator.Send(new SaveImage(file)); }
                catch (ArgumentException exception)
                {
                    return Results.BadRequest(ResponseData<Movie>.Error(exception.Message));
                }
                newMovie.MimeType = file.ContentType;
            }

            try
            {
                db.Movies.Add(newMovie);
                await db.SaveChangesAsync();
            }
            catch
            {
                DeleteImage.IfUploaded(newMovie.Image, environment);
                throw;
            }
            return Results.Created($"/api/movies/{newMovie.Id}", ResponseData<Movie>.Success(newMovie));
        }).WithName("CreateMovie");

        group.MapPut("/{id:int}", async (
            int id,
            [FromForm] string movie,
            [FromForm] IFormFile? file,
            AppDbContext db,
            IMediator mediator,
            IWebHostEnvironment environment) =>
        {
            Movie? editedMovie;
            try { editedMovie = JsonSerializer.Deserialize<Movie>(movie); }
            catch (JsonException) { return Results.BadRequest(ResponseData<Movie>.Error("Некорректные данные фильма.")); }

            if (editedMovie is null || string.IsNullOrWhiteSpace(editedMovie.Name) ||
                string.IsNullOrWhiteSpace(editedMovie.Description) || editedMovie.Price <= 0)
            {
                return Results.BadRequest(ResponseData<Movie>.Error("Заполните название, описание и цену."));
            }

            var current = await db.Movies.FindAsync(id);
            if (current is null)
            {
                return Results.NotFound(ResponseData<Movie>.Error("Фильм не найден."));
            }

            if (!await db.Genres.AnyAsync(genre => genre.Id == editedMovie.GenreId))
            {
                return Results.BadRequest(ResponseData<Movie>.Error("Указанный жанр не существует."));
            }

            var previousImage = current.Image;
            current.Name = editedMovie.Name;
            current.Description = editedMovie.Description;
            current.Price = editedMovie.Price;
            current.GenreId = editedMovie.GenreId;
            if (file is not null)
            {
                try { current.Image = await mediator.Send(new SaveImage(file)); }
                catch (ArgumentException exception)
                {
                    return Results.BadRequest(ResponseData<Movie>.Error(exception.Message));
                }
                current.MimeType = file.ContentType;
            }

            try { await db.SaveChangesAsync(); }
            catch
            {
                if (file is not null) DeleteImage.IfUploaded(current.Image, environment);
                throw;
            }
            if (file is not null) DeleteImage.IfUploaded(previousImage, environment);
            return Results.NoContent();
        }).WithName("UpdateMovie");

        group.MapDelete("/{id:int}", async (int id, AppDbContext db, IWebHostEnvironment environment) =>
        {
            var movie = await db.Movies.FindAsync(id);
            if (movie is null)
            {
                return Results.NotFound(ResponseData<Movie>.Error("Фильм не найден."));
            }

            db.Movies.Remove(movie);
            await db.SaveChangesAsync();
            DeleteImage.IfUploaded(movie.Image, environment);
            return Results.NoContent();
        }).WithName("DeleteMovie");
    }

    private static async Task<IResult> GetAllMovies(
        IMediator mediator,
        string? genre = null,
        int pageNo = 1,
        int pageSize = 3)
    {
        var data = await mediator.Send(new GetListOfMovies(genre, pageNo, pageSize));
        return TypedResults.Ok(data);
    }
}
