namespace MasivMovies.Application.DTOs;

/// <summary>
/// DTO para el reporte mensual de funciones exitosas.
/// </summary>
public sealed class MonthlyReportResponse
{
    public int Year { get; set; }
    public int Month { get; set; }
    public List<ShowtimeReportItem> Showtimes { get; set; } = new();
}

/// <summary>
/// Detalle de una función en el reporte mensual.
/// </summary>
public sealed class ShowtimeReportItem
{
    public Guid ShowtimeId { get; set; }
    public Guid MovieId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public Guid RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public int TicketsSold { get; set; }
}
