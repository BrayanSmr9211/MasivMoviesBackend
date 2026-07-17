using MasivMovies.Domain.Entities;

namespace MasivMovies.Domain.Interfaces;

/// <summary>
/// Contrato para el repositorio de usuarios.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByUsernameAsync(string username);
    Task AddAsync(User user);
    Task<bool> ExistsByUsernameAsync(string username);
}
