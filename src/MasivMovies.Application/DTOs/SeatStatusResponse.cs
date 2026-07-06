namespace MasivMovies.Application.DTOs;

/// <summary>
/// DTO con el estado de disponibilidad de asientos de una función.
/// </summary>
public sealed class SeatStatusResponse
{
    public Guid ShowtimeId { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public int ReservedSeats { get; set; }
    public int SoldSeats { get; set; }
    public List<SeatDetail> Seats { get; set; } = new();
}

/// <summary>
/// Detalle individual de un asiento.
/// </summary>
public sealed class SeatDetail
{
    public int SeatNumber { get; set; }
    public string Status { get; set; } = string.Empty;
}
