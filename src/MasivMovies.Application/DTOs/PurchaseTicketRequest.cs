using System.ComponentModel.DataAnnotations;

namespace MasivMovies.Application.DTOs;

/// <summary>
/// DTO para la compra de un boleto.
/// </summary>
public sealed class PurchaseTicketRequest
{
    [Required]
    public Guid ShowtimeId { get; set; }

    [Required]
    [Range(1, 1000)]
    public int SeatNumber { get; set; }

    [Required]
    public Guid UserId { get; set; }
}
