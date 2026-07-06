using MasivMovies.Application.DTOs;
using MasivMovies.Domain.Interfaces;

namespace MasivMovies.Application.Services;

/// <summary>
/// Servicio de aplicación para la generación de reportes mensuales.
/// </summary>
public sealed class ReportService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IMovieRepository _movieRepository;

    public ReportService(
        ITicketRepository ticketRepository,
        IShowtimeRepository showtimeRepository,
        IMovieRepository movieRepository)
    {
        _ticketRepository = ticketRepository;
        _showtimeRepository = showtimeRepository;
        _movieRepository = movieRepository;
    }

    /// <summary>
    /// Genera un reporte mensual con las funciones más exitosas ordenadas por boletos vendidos.
    /// </summary>
    public async Task<MonthlyReportResponse> GetMonthlyReportAsync(int year, int month)
    {
        if (month < 1 || month > 12)
        {
            throw new InvalidOperationException("El mes debe estar entre 1 y 12.");
        }

        if (year < 2000 || year > 2100)
        {
            throw new InvalidOperationException("El año debe estar entre 2000 y 2100.");
        }

        var ticketData = await _ticketRepository.GetMonthlyReportAsync(year, month);
        var reportItems = new List<ShowtimeReportItem>();

        foreach (var (showtimeId, ticketsSold) in ticketData)
        {
            var showtime = await _showtimeRepository.GetByIdAsync(showtimeId);
            if (showtime is null) continue;

            var movie = await _movieRepository.GetByIdAsync(showtime.MovieId);

            reportItems.Add(new ShowtimeReportItem
            {
                ShowtimeId = showtimeId,
                MovieId = showtime.MovieId,
                MovieTitle = movie?.Title ?? "Desconocida",
                RoomId = showtime.RoomId,
                StartTime = showtime.StartTime,
                TicketsSold = ticketsSold
            });
        }

        return new MonthlyReportResponse
        {
            Year = year,
            Month = month,
            Showtimes = reportItems.OrderByDescending(x => x.TicketsSold).ToList()
        };
    }
}
