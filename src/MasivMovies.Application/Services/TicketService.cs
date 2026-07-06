using MasivMovies.Application.DTOs;
using MasivMovies.Application.Interfaces;
using MasivMovies.Domain.Entities;
using MasivMovies.Domain.Interfaces;

namespace MasivMovies.Application.Services;

/// <summary>
/// Servicio de aplicación para la compra y verificación de boletos.
/// </summary>
public sealed class TicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly ISeatLockService _seatLockService;
    private readonly IMessagePublisher _messagePublisher;

    public TicketService(
        ITicketRepository ticketRepository,
        IShowtimeRepository showtimeRepository,
        IRoomRepository roomRepository,
        ISeatLockService seatLockService,
        IMessagePublisher messagePublisher)
    {
        _ticketRepository = ticketRepository;
        _showtimeRepository = showtimeRepository;
        _roomRepository = roomRepository;
        _seatLockService = seatLockService;
        _messagePublisher = messagePublisher;
    }

    /// <summary>
    /// Intenta comprar un boleto para una función específica.
    /// Usa Redis para manejar la concurrencia (solo una compra por asiento).
    /// </summary>
    public async Task<Ticket> PurchaseTicketAsync(PurchaseTicketRequest request)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(request.ShowtimeId);
        if (showtime is null)
        {
            throw new InvalidOperationException($"La función con Id '{request.ShowtimeId}' no existe.");
        }

        if (showtime.IsCancelled)
        {
            throw new InvalidOperationException("No se pueden comprar boletos para una función cancelada.");
        }

        var room = await _roomRepository.GetByIdAsync(showtime.RoomId);
        if (room is null)
        {
            throw new InvalidOperationException("La sala asociada a la función no existe.");
        }

        if (request.SeatNumber < 1 || request.SeatNumber > room.TotalSeats)
        {
            throw new InvalidOperationException(
                $"El número de asiento debe estar entre 1 y {room.TotalSeats}.");
        }

        var locked = await _seatLockService.TryLockSeatAsync(
            request.ShowtimeId, request.SeatNumber, request.UserId);

        if (!locked)
        {
            throw new InvalidOperationException(
                $"El asiento {request.SeatNumber} no está disponible. Puede estar reservado o ya vendido.");
        }

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            ShowtimeId = request.ShowtimeId,
            SeatNumber = request.SeatNumber,
            UserId = request.UserId,
            Status = TicketStatus.Reserved
        };

        await _ticketRepository.AddAsync(ticket);

        await _messagePublisher.PublishAsync("ticket-purchases", new
        {
            TicketId = ticket.Id,
            ticket.ShowtimeId,
            ticket.SeatNumber,
            ticket.UserId,
            Status = "Reserved",
            Timestamp = DateTime.UtcNow
        });

        return ticket;
    }

    /// <summary>
    /// Confirma la compra de un boleto previamente reservado.
    /// </summary>
    public async Task<Ticket> ConfirmPurchaseAsync(Guid ticketId, Guid userId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);
        if (ticket is null)
        {
            throw new InvalidOperationException($"El boleto con Id '{ticketId}' no existe.");
        }

        if (ticket.UserId != userId)
        {
            throw new InvalidOperationException("No tiene permisos para confirmar este boleto.");
        }

        if (ticket.Status != TicketStatus.Reserved)
        {
            throw new InvalidOperationException("Solo se pueden confirmar boletos en estado reservado.");
        }

        var confirmed = await _seatLockService.ConfirmSeatAsync(
            ticket.ShowtimeId, ticket.SeatNumber, userId);

        if (!confirmed)
        {
            throw new InvalidOperationException("La reserva ha expirado. Intente nuevamente.");
        }

        ticket.Status = TicketStatus.Sold;
        ticket.ConfirmedAt = DateTime.UtcNow;
        await _ticketRepository.UpdateAsync(ticket);

        await _messagePublisher.PublishAsync("ticket-confirmations", new
        {
            ticket.Id,
            ticket.ShowtimeId,
            ticket.SeatNumber,
            ticket.UserId,
            Status = "Sold",
            Timestamp = DateTime.UtcNow
        });

        return ticket;
    }

    /// <summary>
    /// Obtiene el estado de disponibilidad de asientos para una función.
    /// </summary>
    public async Task<SeatStatusResponse> GetSeatStatusAsync(Guid showtimeId)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(showtimeId);
        if (showtime is null)
        {
            throw new InvalidOperationException($"La función con Id '{showtimeId}' no existe.");
        }

        var room = await _roomRepository.GetByIdAsync(showtime.RoomId);
        if (room is null)
        {
            throw new InvalidOperationException("La sala asociada no existe.");
        }

        var seats = new List<SeatDetail>();
        int available = 0, reserved = 0, sold = 0;

        for (int i = 1; i <= room.TotalSeats; i++)
        {
            var status = await _seatLockService.GetSeatStatusAsync(showtimeId, i);
            seats.Add(new SeatDetail { SeatNumber = i, Status = status });

            switch (status)
            {
                case "Available":
                    available++;
                    break;
                case "Reserved":
                    reserved++;
                    break;
                case "Sold":
                    sold++;
                    break;
            }
        }

        return new SeatStatusResponse
        {
            ShowtimeId = showtimeId,
            TotalSeats = room.TotalSeats,
            AvailableSeats = available,
            ReservedSeats = reserved,
            SoldSeats = sold,
            Seats = seats
        };
    }
}
