using MasivMovies.Application.DTOs;
using MasivMovies.Application.Interfaces;
using MasivMovies.Application.Services;
using MasivMovies.Domain.Entities;
using MasivMovies.Domain.Interfaces;
using Moq;

namespace MasivMovies.Application.Tests;

public sealed class ShowtimeServiceTests
{
    private readonly Mock<IShowtimeRepository> _showtimeRepoMock;
    private readonly Mock<IMovieRepository> _movieRepoMock;
    private readonly Mock<IRoomRepository> _roomRepoMock;
    private readonly Mock<ISeatLockService> _seatLockMock;
    private readonly ShowtimeService _service;

    public ShowtimeServiceTests()
    {
        _showtimeRepoMock = new Mock<IShowtimeRepository>();
        _movieRepoMock = new Mock<IMovieRepository>();
        _roomRepoMock = new Mock<IRoomRepository>();
        _seatLockMock = new Mock<ISeatLockService>();

        _service = new ShowtimeService(
            _showtimeRepoMock.Object,
            _movieRepoMock.Object,
            _roomRepoMock.Object,
            _seatLockMock.Object);
    }

    [Fact]
    public async Task CreateShowtimeAsync_WhenValid_ShouldCreateSuccessfully()
    {
        // Given
        var request = new CreateShowtimeRequest
        {
            MovieId = Guid.NewGuid(),
            RoomId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2)
        };

        _movieRepoMock.Setup(r => r.ExistsAsync(request.MovieId)).ReturnsAsync(true);
        _roomRepoMock.Setup(r => r.ExistsAsync(request.RoomId)).ReturnsAsync(true);
        _showtimeRepoMock
            .Setup(r => r.GetByRoomAndDateRangeAsync(request.RoomId, request.StartTime, request.EndTime))
            .ReturnsAsync(Enumerable.Empty<Showtime>());
        _showtimeRepoMock.Setup(r => r.AddAsync(It.IsAny<Showtime>())).Returns(Task.CompletedTask);

        // When
        var result = await _service.CreateShowtimeAsync(request);

        // Then
        Assert.NotNull(result);
        Assert.Equal(request.MovieId, result.MovieId);
        Assert.Equal(request.RoomId, result.RoomId);
        Assert.False(result.IsCancelled);
    }

    [Fact]
    public async Task CreateShowtimeAsync_WhenEndBeforeStart_ShouldThrow()
    {
        // Given
        var request = new CreateShowtimeRequest
        {
            MovieId = Guid.NewGuid(),
            RoomId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            EndTime = DateTime.UtcNow.AddDays(1)
        };

        // When & Then
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateShowtimeAsync(request));

        Assert.Contains("posterior", exception.Message);
    }

    [Fact]
    public async Task CancelShowtimeAsync_WhenExists_ShouldCancelAndReleaseSeats()
    {
        // Given
        var showtimeId = Guid.NewGuid();
        var roomId = Guid.NewGuid();
        var showtime = new Showtime { Id = showtimeId, RoomId = roomId, IsCancelled = false };
        var room = new Room { Id = roomId, TotalSeats = 80 };

        _showtimeRepoMock.Setup(r => r.GetByIdAsync(showtimeId)).ReturnsAsync(showtime);
        _roomRepoMock.Setup(r => r.GetByIdAsync(roomId)).ReturnsAsync(room);
        _seatLockMock.Setup(s => s.ReleaseAllSeatsAsync(showtimeId, 80)).Returns(Task.CompletedTask);
        _showtimeRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Showtime>())).Returns(Task.CompletedTask);

        // When
        await _service.CancelShowtimeAsync(showtimeId);

        // Then
        _seatLockMock.Verify(s => s.ReleaseAllSeatsAsync(showtimeId, 80), Times.Once);
        _showtimeRepoMock.Verify(r => r.UpdateAsync(It.Is<Showtime>(s => s.IsCancelled)), Times.Once);
    }
}
