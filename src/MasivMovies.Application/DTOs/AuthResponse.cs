namespace MasivMovies.Application.DTOs;

/// <summary>
/// DTO de respuesta de autenticación con token JWT.
/// </summary>
public sealed class AuthResponse
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
