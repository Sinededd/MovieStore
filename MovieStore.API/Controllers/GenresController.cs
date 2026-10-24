using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieStore.API.Data;
using MovieStore.Domain.Entities;
using MovieStore.Domain.Models;

namespace MovieStore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenresController(AppDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ResponseData<List<Genre>>> GetGenres() =>
        ResponseData<List<Genre>>.Success(
            await context.Genres.AsNoTracking().OrderBy(genre => genre.Id).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ResponseData<Genre>>> GetGenre(int id)
    {
        var genre = await context.Genres.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return genre is null
            ? NotFound(ResponseData<Genre>.Error("Жанр не найден."))
            : Ok(ResponseData<Genre>.Success(genre));
    }

    [HttpPost]
    public async Task<ActionResult<ResponseData<Genre>>> PostGenre(Genre genre)
    {
        genre.Id = 0;
        context.Genres.Add(genre);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetGenre), new { id = genre.Id }, ResponseData<Genre>.Success(genre));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> PutGenre(int id, Genre genre)
    {
        var current = await context.Genres.FindAsync(id);
        if (current is null)
        {
            return NotFound(ResponseData<Genre>.Error("Жанр не найден."));
        }

        current.Name = genre.Name;
        current.NormalizedName = genre.NormalizedName;
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteGenre(int id)
    {
        var genre = await context.Genres.FindAsync(id);
        if (genre is null)
        {
            return NotFound(ResponseData<Genre>.Error("Жанр не найден."));
        }

        context.Genres.Remove(genre);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
