using MasivMovies.Domain.Entities;

namespace MasivMovies.Domain.Interfaces;

/// <summary>
/// Contrato para el repositorio de funciones.
/// </summary>
public interface IShowtimeRepository
{
    Task<Showtime?> GetByIdAsync(Guid id);
    Task<IEnumerable<Showtime>> GetAllAsync();
    Task<IEnumerable<Showtime>> GetByRoomAndDateRangeAsync(Guid roomId, DateTime start, DateTime end);
    Task AddAsync(Showtime showtime);
    Task UpdateAsync(Showtime showtime);
    Task<bool> ExistsAsync(Guid id);
}
