namespace MasivMovies.Domain.Entities;

/// <summary>
/// Representa una sala de cine dentro de una sede.
/// </summary>
public sealed class Room
{
    public Guid Id { get; set; }
    public Guid CinemaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
