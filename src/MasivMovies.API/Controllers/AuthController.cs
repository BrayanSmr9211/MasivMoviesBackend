using MasivMovies.Application.DTOs;
using MasivMovies.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace MasivMovies.API.Controllers;

/// <summary>
/// Controller para autenticación y registro de usuarios.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(AuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuevo usuario y retorna un token JWT.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var response = await _authService.RegisterAsync(request);
        _logger.LogInformation("Usuario registrado: {Username}", request.Username);
        return CreatedAtAction(nameof(Register), response);
    }

    /// <summary>
    /// Autentica un usuario y retorna un token JWT.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        _logger.LogInformation("Login exitoso: {Username}", request.Username);
        return Ok(response);
    }
}
