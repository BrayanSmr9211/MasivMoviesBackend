using MasivMovies.Application.DTOs;
using MasivMovies.Application.Interfaces;
using MasivMovies.Domain.Entities;
using MasivMovies.Domain.Interfaces;

namespace MasivMovies.Application.Services;

/// <summary>
/// Servicio de aplicación para la gestión de funciones (showtimes).
/// </summary>
public sealed class ShowtimeService
{
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly ISeatLockService _seatLockService;

    public ShowtimeService(
        IShowtimeRepository showtimeRepository,
        IMovieRepository movieRepository,
        IRoomRepository roomRepository,
        ISeatLockService seatLockService)
    {
        _showtimeRepository = showtimeRepository;
        _movieRepository = movieRepository;
        _roomRepository = roomRepository;
        _seatLockService = seatLockService;
    }

    /// <summary>
    /// Registra un nuevo horario de función.
    /// </summary>
    public async Task<Showtime> CreateShowtimeAsync(CreateShowtimeRequest request)
    {
        if (request.EndTime <= request.StartTime)
        {
            throw new InvalidOperationException("La hora de finalización debe ser posterior a la hora de inicio.");
        }

        var movieExists = await _movieRepository.ExistsAsync(request.MovieId);
        if (!movieExists)
        {
            throw new InvalidOperationException($"La película con Id '{request.MovieId}' no existe.");
        }

        var roomExists = await _roomRepository.ExistsAsync(request.RoomId);
        if (!roomExists)
        {
            throw new InvalidOperationException($"La sala con Id '{request.RoomId}' no existe.");
        }

        var conflicting = await _showtimeRepository.GetByRoomAndDateRangeAsync(
            request.RoomId, request.StartTime, request.EndTime);

        if (conflicting.Any(s => !s.IsCancelled))
        {
            throw new InvalidOperationException("Existe un conflicto de horarios con otra función en esta sala.");
        }

        var showtime = new Showtime
        {
            Id = Guid.NewGuid(),
            MovieId = request.MovieId,
            RoomId = request.RoomId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            IsCancelled = false
        };

        await _showtimeRepository.AddAsync(showtime);
        return showtime;
    }

    /// <summary>
    /// Cancela una función programada y libera los asientos reservados.
    /// </summary>
    public async Task CancelShowtimeAsync(Guid showtimeId)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(showtimeId);
        if (showtime is null)
        {
            throw new InvalidOperationException($"La función con Id '{showtimeId}' no existe.");
        }

        if (showtime.IsCancelled)
        {
            throw new InvalidOperationException("La función ya se encuentra cancelada.");
        }

        var room = await _roomRepository.GetByIdAsync(showtime.RoomId);
        if (room is not null)
        {
            await _seatLockService.ReleaseAllSeatsAsync(showtimeId, room.TotalSeats);
        }

        showtime.IsCancelled = true;
        await _showtimeRepository.UpdateAsync(showtime);
    }

    /// <summary>
    /// Obtiene todas las funciones registradas.
    /// </summary>
    public async Task<IEnumerable<Showtime>> GetAllShowtimesAsync()
    {
        return await _showtimeRepository.GetAllAsync();
    }
}
