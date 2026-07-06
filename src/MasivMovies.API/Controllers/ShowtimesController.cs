using MasivMovies.Application.DTOs;
using MasivMovies.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MasivMovies.API.Controllers;

/// <summary>
/// Controller para la gestión de funciones (horarios de proyección).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class ShowtimesController : ControllerBase
{
    private readonly ShowtimeService _showtimeService;
    private readonly ILogger<ShowtimesController> _logger;

    public ShowtimesController(ShowtimeService showtimeService, ILogger<ShowtimesController> logger)
    {
        _showtimeService = showtimeService;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuevo horario de función.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateShowtimeRequest request)
    {
        try
        {
            var showtime = await _showtimeService.CreateShowtimeAsync(request);
            _logger.LogInformation("Función creada: {ShowtimeId}", showtime.Id);
            return CreatedAtAction(nameof(GetAll), null, showtime);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { detail = ex.Message });
        }
    }

    /// <summary>
    /// Cancela una función programada.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            await _showtimeService.CancelShowtimeAsync(id);
            _logger.LogInformation("Función cancelada: {ShowtimeId}", id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { detail = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todas las funciones registradas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var showtimes = await _showtimeService.GetAllShowtimesAsync();
        return Ok(showtimes);
    }
}
