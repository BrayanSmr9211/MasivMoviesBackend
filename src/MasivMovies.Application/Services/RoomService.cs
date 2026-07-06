using MasivMovies.Application.DTOs;
using MasivMovies.Domain.Entities;
using MasivMovies.Domain.Interfaces;

namespace MasivMovies.Application.Services;

/// <summary>
/// Servicio de aplicación para la gestión de salas de cine.
/// </summary>
public sealed class RoomService
{
    private readonly IRoomRepository _roomRepository;

    public RoomService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    /// <summary>
    /// Registra una nueva sala de cine.
    /// </summary>
    public async Task<Room> CreateRoomAsync(CreateRoomRequest request)
    {
        var exists = await _roomRepository.ExistsAsync(request.Id);
        if (exists)
        {
            throw new InvalidOperationException($"La sala con Id '{request.Id}' ya existe.");
        }

        var room = new Room
        {
            Id = request.Id,
            CinemaId = request.CinemaId,
            Name = request.Name,
            TotalSeats = request.TotalSeats,
            Description = request.Description
        };

        await _roomRepository.AddAsync(room);
        return room;
    }

    /// <summary>
    /// Obtiene todas las salas registradas.
    /// </summary>
    public async Task<IEnumerable<Room>> GetAllRoomsAsync()
    {
        return await _roomRepository.GetAllAsync();
    }
}
