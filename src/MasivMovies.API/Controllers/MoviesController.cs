using MasivMovies.Application.DTOs;
using MasivMovies.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MasivMovies.API.Controllers;

/// <summary>
/// Controller para la gestión de películas.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class MoviesController : ControllerBase
{
    private readonly MovieService _movieService;
    private readonly ILogger<MoviesController> _logger;

    public MoviesController(MovieService movieService, ILogger<MoviesController> logger)
    {
        _movieService = movieService;
        _logger = logger;
    }

    /// <summary>
    /// Crea una nueva película.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        var movie = await _movieService.CreateMovieAsync(request);
        _logger.LogInformation("Película creada: {MovieId}", movie.Id);
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }

    /// <summary>
    /// Obtiene todas las películas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var movies = await _movieService.GetAllMoviesAsync();
        return Ok(movies);
    }

    /// <summary>
    /// Obtiene una película por su Id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var movie = await _movieService.GetMovieByIdAsync(id);
        if (movie is null)
        {
            throw new KeyNotFoundException($"Película con Id '{id}' no encontrada.");
        }
        return Ok(movie);
    }
}
