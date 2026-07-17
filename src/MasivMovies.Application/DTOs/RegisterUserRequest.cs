using System.ComponentModel.DataAnnotations;

namespace MasivMovies.Application.DTOs;

/// <summary>
/// DTO para el registro de un usuario.
/// </summary>
public sealed class RegisterUserRequest
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
}
