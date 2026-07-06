using MasivMovies.Application.DTOs;
using MasivMovies.Application.Services;
using MasivMovies.Domain.Entities;
using MasivMovies.Domain.Interfaces;
using Moq;

namespace MasivMovies.Application.Tests;

public sealed class MovieServiceTests
{
    private readonly Mock<IMovieRepository> _movieRepoMock;
    private readonly MovieService _service;

    public MovieServiceTests()
    {
        _movieRepoMock = new Mock<IMovieRepository>();
        _service = new MovieService(_movieRepoMock.Object);
    }

    [Fact]
    public async Task CreateMovieAsync_WhenMovieDoesNotExist_ShouldCreateSuccessfully()
    {
        // Given
        var request = new CreateMovieRequest
        {
            Id = Guid.NewGuid(),
            Title = "Inception",
            Director = "Christopher Nolan",
            Genre = "Sci-Fi",
            DurationMinutes = 148,
            Synopsis = "A thief who enters dreams",
            ReleaseYear = 2010,
            AgeRating = "PG-13"
        };

        _movieRepoMock.Setup(r => r.ExistsAsync(request.Id)).ReturnsAsync(false);
        _movieRepoMock.Setup(r => r.AddAsync(It.IsAny<Movie>())).Returns(Task.CompletedTask);

        // When
        var result = await _service.CreateMovieAsync(request);

        // Then
        Assert.NotNull(result);
        Assert.Equal(request.Id, result.Id);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Director, result.Director);
        _movieRepoMock.Verify(r => r.AddAsync(It.IsAny<Movie>()), Times.Once);
    }

    [Fact]
    public async Task CreateMovieAsync_WhenMovieAlreadyExists_ShouldThrowInvalidOperation()
    {
        // Given
        var request = new CreateMovieRequest
        {
            Id = Guid.NewGuid(),
            Title = "Inception",
            Director = "Christopher Nolan",
            Genre = "Sci-Fi",
            DurationMinutes = 148,
            Synopsis = "A thief who enters dreams",
            ReleaseYear = 2010,
            AgeRating = "PG-13"
        };

        _movieRepoMock.Setup(r => r.ExistsAsync(request.Id)).ReturnsAsync(true);

        // When & Then
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateMovieAsync(request));

        Assert.Contains("ya existe", exception.Message);
    }

    [Fact]
    public async Task GetAllMoviesAsync_ShouldReturnAllMovies()
    {
        // Given
        var movies = new List<Movie>
        {
            new() { Id = Guid.NewGuid(), Title = "Movie 1" },
            new() { Id = Guid.NewGuid(), Title = "Movie 2" }
        };

        _movieRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(movies);

        // When
        var result = await _service.GetAllMoviesAsync();

        // Then
        Assert.Equal(2, result.Count());
    }
}
