using System.ComponentModel.DataAnnotations;

namespace MasivMovies.Application.DTOs;

/// <summary>
/// DTO para el login de un usuario.
/// </summary>
public sealed class LoginRequest
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
