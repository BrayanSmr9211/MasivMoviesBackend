using MasivMovies.Domain.Entities;

namespace MasivMovies.Domain.Interfaces;

/// <summary>
/// Contrato para el repositorio de salas.
/// </summary>
public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id);
    Task<IEnumerable<Room>> GetAllAsync();
    Task AddAsync(Room room);
    Task<bool> ExistsAsync(Guid id);
}
