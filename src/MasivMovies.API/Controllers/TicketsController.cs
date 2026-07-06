using MasivMovies.Application.DTOs;
using MasivMovies.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MasivMovies.API.Controllers;

/// <summary>
/// Controller para la compra y verificación de boletos.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
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
    /// Compra un boleto para una función específica.
    /// </summary>
    [HttpPost("purchase")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Purchase([FromBody] PurchaseTicketRequest request)
    {
        try
        {
            var ticket = await _ticketService.PurchaseTicketAsync(request);
            _logger.LogInformation("Boleto reservado: {TicketId}", ticket.Id);
            return CreatedAtAction(nameof(GetSeatStatus), new { showtimeId = ticket.ShowtimeId }, ticket);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { detail = ex.Message });
        }
    }

    /// <summary>
    /// Confirma la compra de un boleto previamente reservado.
    /// </summary>
    [HttpPost("{ticketId:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Confirm(Guid ticketId, [FromQuery] Guid userId)
    {
        try
        {
            var ticket = await _ticketService.ConfirmPurchaseAsync(ticketId, userId);
            _logger.LogInformation("Boleto confirmado: {TicketId}", ticket.Id);
            return Ok(ticket);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { detail = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene el estado de disponibilidad de asientos para una función.
    /// </summary>
    [HttpGet("seats/{showtimeId:guid}")]
    [ProducesResponseType(typeof(SeatStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSeatStatus(Guid showtimeId)
    {
        try
        {
            var status = await _ticketService.GetSeatStatusAsync(showtimeId);
            return Ok(status);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { detail = ex.Message });
        }
    }
}
