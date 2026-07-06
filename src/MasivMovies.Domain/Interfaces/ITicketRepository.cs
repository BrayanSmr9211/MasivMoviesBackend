using MasivMovies.Domain.Entities;

namespace MasivMovies.Domain.Interfaces;

/// <summary>
/// Contrato para el repositorio de boletos.
/// </summary>
public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id);
    Task<IEnumerable<Ticket>> GetByShowtimeIdAsync(Guid showtimeId);
    Task AddAsync(Ticket ticket);
    Task UpdateAsync(Ticket ticket);
    Task DeleteByShowtimeIdAsync(Guid showtimeId);
    Task<int> GetSoldTicketsCountByShowtimeAsync(Guid showtimeId);
    Task<IEnumerable<(Guid ShowtimeId, int TicketsSold)>> GetMonthlyReportAsync(int year, int month);
}
