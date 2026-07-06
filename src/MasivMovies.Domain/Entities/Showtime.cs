namespace MasivMovies.Domain.Entities;

/// <summary>
/// Representa una función programada de una película en una sala.
/// </summary>
public sealed class Showtime
{
    public Guid Id { get; set; }
    public Guid MovieId { get; set; }
    public Guid RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsCancelled { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
