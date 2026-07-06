using System.ComponentModel.DataAnnotations;

namespace MasivMovies.Application.DTOs;

/// <summary>
/// DTO para el registro de una función (horario de proyección).
/// </summary>
public sealed class CreateShowtimeRequest
{
    [Required]
    public Guid MovieId { get; set; }

    [Required]
    public Guid RoomId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }
}
