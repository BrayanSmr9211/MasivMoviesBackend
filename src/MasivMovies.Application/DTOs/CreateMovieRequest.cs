using System.ComponentModel.DataAnnotations;

namespace MasivMovies.Application.DTOs;

/// <summary>
/// DTO para la creación de una película.
/// </summary>
public sealed class CreateMovieRequest
{
    [Required]
    public Guid Id { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Director { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Genre { get; set; } = string.Empty;

    [Required]
    [Range(1, 600)]
    public int DurationMinutes { get; set; }

    [Required]
    [StringLength(1000, MinimumLength = 1)]
    public string Synopsis { get; set; } = string.Empty;

    [Required]
    [Range(1888, 2100)]
    public int ReleaseYear { get; set; }

    [Required]
    [StringLength(10, MinimumLength = 1)]
    public string AgeRating { get; set; } = string.Empty;
}
