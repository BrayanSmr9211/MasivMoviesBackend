using System.Security.Claims;
using MasivMovies.Application.DTOs;
using MasivMovies.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MasivMovies.API.Controllers;

/// <summary>
/// Controller para la compra y verificación de boletos. Requiere autenticación JWT.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public sealed class TicketsController : ControllerBase
{
    private readonly TicketService _ticketService;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(TicketService ticketService, ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    /// <summary>
    /// Compra un boleto para una función específica. El UserId se toma del token JWT.
    /// </summary>
    [HttpPost("purchase")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Purchase([FromBody] PurchaseTicketRequest request)
    {
        var userId = GetUserIdFromToken();
        request.UserId = userId;

        var ticket = await _ticketService.PurchaseTicketAsync(request);
        _logger.LogInformation("Boleto reservado: {TicketId}", ticket.Id);
        return CreatedAtAction(nameof(GetSeatStatus), new { showtimeId = ticket.ShowtimeId }, ticket);
    }

    /// <summary>
    /// Confirma la compra de un boleto previamente reservado.
    /// </summary>
    [HttpPost("{ticketId:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirm(Guid ticketId)
    {
        var userId = GetUserIdFromToken();

        var ticket = await _ticketService.ConfirmPurchaseAsync(ticketId, userId);
        _logger.LogInformation("Boleto confirmado: {TicketId}", ticket.Id);
        return Ok(ticket);
    }

    /// <summary>
    /// Obtiene el estado de disponibilidad de asientos para una función. Endpoint público.
    /// </summary>
    [HttpGet("seats/{showtimeId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SeatStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSeatStatus(Guid showtimeId)
    {
        var status = await _ticketService.GetSeatStatusAsync(showtimeId);
        return Ok(status);
    }

    /// <summary>
    /// Extrae el UserId del claim del token JWT.
    /// </summary>
    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Token inválido o expirado.");
        }
        return userId;
    }
}
