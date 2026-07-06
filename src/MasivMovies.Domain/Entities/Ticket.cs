namespace MasivMovies.Domain.Entities;

/// <summary>
/// Representa un boleto comprado para una función específica.
/// </summary>
public sealed class Ticket
{
    public Guid Id { get; set; }
    public Guid ShowtimeId { get; set; }
    public int SeatNumber { get; set; }
    public Guid UserId { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.Reserved;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAt { get; set; }
}
