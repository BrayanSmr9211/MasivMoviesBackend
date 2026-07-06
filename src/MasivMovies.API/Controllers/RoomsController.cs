using MasivMovies.Application.DTOs;
using MasivMovies.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MasivMovies.API.Controllers;

/// <summary>
/// Controller para la gestión de salas de cine.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class RoomsController : ControllerBase
{
    private readonly RoomService _roomService;
    private readonly ILogger<RoomsController> _logger;

    public RoomsController(RoomService roomService, ILogger<RoomsController> logger)
    {
        _roomService = roomService;
        _logger = logger;
    }

    /// <summary>
    /// Registra una nueva sala de cine.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequest request)
    {
        try
        {
            var room = await _roomService.CreateRoomAsync(request);
            _logger.LogInformation("Sala creada: {RoomId}", room.Id);
            return CreatedAtAction(nameof(GetAll), null, room);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { detail = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todas las salas registradas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var rooms = await _roomService.GetAllRoomsAsync();
        return Ok(rooms);
    }
}
