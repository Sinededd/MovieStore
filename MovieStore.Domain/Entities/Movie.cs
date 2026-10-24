using System.ComponentModel.DataAnnotations;

namespace MovieStore.Domain.Entities;

public class Movie
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, 1_000_000)]
    public decimal Price { get; set; }

    public string? Image { get; set; }
    public string? MimeType { get; set; }

    public int GenreId { get; set; }
    public Genre? Genre { get; set; }
}
