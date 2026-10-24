using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace MovieStore.Domain.Entities;

public class Genre
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string NormalizedName { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Movie> Movies { get; set; } = new List<Movie>();
}
