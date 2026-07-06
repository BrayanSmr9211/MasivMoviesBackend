using MasivMovies.Application.DTOs;
using MasivMovies.Application.Interfaces;
using MasivMovies.Application.Services;
using MasivMovies.Domain.Entities;
using MasivMovies.Domain.Interfaces;
using Moq;

namespace MasivMovies.Application.Tests;

public sealed class TicketServiceTests
{
    private readonly Mock<ITicketRepository> _ticketRepoMock;
    private readonly Mock<IShowtimeRepository> _showtimeRepoMock;
    private readonly Mock<IRoomRepository> _roomRepoMock;
    private readonly Mock<ISeatLockService> _seatLockMock;
    private readonly Mock<IMessagePublisher> _publisherMock;
    private readonly TicketService _service;

    public TicketServiceTests()
    {
        _ticketRepoMock = new Mock<ITicketRepository>();
        _showtimeRepoMock = new Mock<IShowtimeRepository>();
        _roomRepoMock = new Mock<IRoomRepository>();
        _seatLockMock = new Mock<ISeatLockService>();
        _publisherMock = new Mock<IMessagePublisher>();

        _service = new TicketService(
            _ticketRepoMock.Object,
            _showtimeRepoMock.Object,
            _roomRepoMock.Object,
            _seatLockMock.Object,
            _publisherMock.Object);
    }

    [Fact]
    public async Task PurchaseTicketAsync_WhenSeatAvailable_ShouldReserveSuccessfully()
    {
        // Given
        var showtimeId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var showtime = new Showtime { Id = showtimeId, RoomId = roomId, IsCancelled = false };
        var room = new Room { Id = roomId, TotalSeats = 100 };

        _showtimeRepoMock.Setup(r => r.GetByIdAsync(showtimeId)).ReturnsAsync(showtime);
        _roomRepoMock.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _seatLockMock.Setup(s => s.TryLockSeatAsync(showtimeId, 5, userId)).ReturnsAsync(true);
        _ticketRepoMock.Setup(r => r.AddAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
        _publisherMock.Setup(p => p.PublishAsync(It.IsAny<string>(), It.IsAny<object>())).Returns(Task.CompletedTask);

        var request = new PurchaseTicketRequest
        {
            ShowtimeId = showtimeId,
            SeatNumber = 5,
            UserId = userId
        };

        // When
        var result = await _service.PurchaseTicketAsync(request);

        // Then
        Assert.NotNull(result);
        Assert.Equal(TicketStatus.Reserved, result.Status);
        Assert.Equal(5, result.SeatNumber);
        _seatLockMock.Verify(s => s.TryLockSeatAsync(showtimeId, 5, userId), Times.Once);
        _publisherMock.Verify(p => p.PublishAsync("ticket-purchases", It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task PurchaseTicketAsync_WhenSeatAlreadyTaken_ShouldThrowInvalidOperation()
    {
        // Given
        var showtimeId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var showtime = new Showtime { Id = showtimeId, RoomId = roomId, IsCancelled = false };
        var room = new Room { Id = roomId, TotalSeats = 100 };

        _showtimeRepoMock.Setup(r => r.GetByIdAsync(showtimeId)).ReturnsAsync(showtime);
        _roomRepoMock.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _seatLockMock.Setup(s => s.TryLockSeatAsync(showtimeId, 5, userId)).ReturnsAsync(false);

        var request = new PurchaseTicketRequest
        {
            ShowtimeId = showtimeId,
            SeatNumber = 5,
            UserId = userId
        };

        // When & Then
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.PurchaseTicketAsync(request));

        Assert.Contains("no está disponible", exception.Message);
    }

    [Fact]
    public async Task PurchaseTicketAsync_WhenShowtimeCancelled_ShouldThrowInvalidOperation()
    {
        // Given
        var showtimeId = Guid.NewGuid();
        var showtime = new Showtime { Id = showtimeId, IsCancelled = true };

        _showtimeRepoMock.Setup(r => r.GetByIdAsync(showtimeId)).ReturnsAsync(showtime);

        var request = new PurchaseTicketRequest
        {
            ShowtimeId = showtimeId,
            SeatNumber = 1,
            UserId = Guid.NewGuid()
        };

        // When & Then
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.PurchaseTicketAsync(request));

        Assert.Contains("cancelada", exception.Message);
    }

    [Fact]
    public async Task PurchaseTicketAsync_WhenSeatOutOfRange_ShouldThrowInvalidOperation()
    {
        // Given
        var showtimeId = Guid.NewGuid();
        var roomId = Guid.NewGuid();

        var showtime = new Showtime { Id = showtimeId, RoomId = roomId, IsCancelled = false };
        var room = new Room { Id = roomId, TotalSeats = 50 };

        _showtimeRepoMock.Setup(r => r.GetByIdAsync(showtimeId)).ReturnsAsync(showtime);
        _roomRepoMock.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);

        var request = new PurchaseTicketRequest
        {
            ShowtimeId = showtimeId,
            SeatNumber = 51,
            UserId = Guid.NewGuid()
        };

        // When & Then
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.PurchaseTicketAsync(request));

        Assert.Contains("entre 1 y 50", exception.Message);
    }
}
