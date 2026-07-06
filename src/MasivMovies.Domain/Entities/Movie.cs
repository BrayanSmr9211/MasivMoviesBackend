namespace MasivMovies.Domain.Entities;

/// <summary>
/// Representa una película disponible en el sistema de cine.
/// </summary>
public sealed class Movie
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Director { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Synopsis { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public string AgeRating { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
