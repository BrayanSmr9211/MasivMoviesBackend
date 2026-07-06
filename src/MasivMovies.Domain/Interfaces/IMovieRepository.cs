using MasivMovies.Domain.Entities;

namespace MasivMovies.Domain.Interfaces;

/// <summary>
/// Contrato para el repositorio de películas.
/// </summary>
public interface IMovieRepository
{
    Task<Movie?> GetByIdAsync(Guid id);
    Task<IEnumerable<Movie>> GetAllAsync();
    Task AddAsync(Movie movie);
    Task<bool> ExistsAsync(Guid id);
}
