using MasivMovies.Application.DTOs;
using MasivMovies.Domain.Entities;
using MasivMovies.Domain.Interfaces;

namespace MasivMovies.Application.Services;

/// <summary>
/// Servicio de aplicación para la gestión de películas.
/// </summary>
public sealed class MovieService
{
    private readonly IMovieRepository _movieRepository;

    public MovieService(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    /// <summary>
    /// Crea una nueva película en el sistema.
    /// </summary>
    public async Task<Movie> CreateMovieAsync(CreateMovieRequest request)
    {
        var exists = await _movieRepository.ExistsAsync(request.Id);
        if (exists)
        {
            throw new InvalidOperationException($"La película con Id '{request.Id}' ya existe.");
        }

        var movie = new Movie
        {
            Id = request.Id,
            Title = request.Title,
            Director = request.Director,
            Genre = request.Genre,
            DurationMinutes = request.DurationMinutes,
            Synopsis = request.Synopsis,
            ReleaseYear = request.ReleaseYear,
            AgeRating = request.AgeRating
        };

        await _movieRepository.AddAsync(movie);
        return movie;
    }

    /// <summary>
    /// Obtiene todas las películas registradas.
    /// </summary>
    public async Task<IEnumerable<Movie>> GetAllMoviesAsync()
    {
        return await _movieRepository.GetAllAsync();
    }

    /// <summary>
    /// Obtiene una película por su Id.
    /// </summary>
    public async Task<Movie?> GetMovieByIdAsync(Guid id)
    {
        return await _movieRepository.GetByIdAsync(id);
    }
}
