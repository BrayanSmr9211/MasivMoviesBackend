using MasivMovies.Domain.Entities;

namespace MasivMovies.Application.Interfaces;

/// <summary>
/// Contrato para la generación y validación de tokens JWT.
/// </summary>
public interface IJwtService
{
    string GenerateToken(User user);
}
