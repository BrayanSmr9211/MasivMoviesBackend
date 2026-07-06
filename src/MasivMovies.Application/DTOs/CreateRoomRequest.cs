using System.ComponentModel.DataAnnotations;

namespace MasivMovies.Application.DTOs;

/// <summary>
/// DTO para el registro de una sala de cine.
/// </summary>
public sealed class CreateRoomRequest
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    public Guid CinemaId { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000)]
    public int TotalSeats { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}
