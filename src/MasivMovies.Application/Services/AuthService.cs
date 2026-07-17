using System.Security.Cryptography;
using System.Text;
using MasivMovies.Application.DTOs;
using MasivMovies.Application.Interfaces;
using MasivMovies.Domain.Entities;
using MasivMovies.Domain.Interfaces;

namespace MasivMovies.Application.Services;

/// <summary>
/// Servicio de aplicación para autenticación y registro de usuarios.
/// </summary>
public sealed class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    public async Task<AuthResponse> RegisterAsync(RegisterUserRequest request)
    {
        var exists = await _userRepository.ExistsByUsernameAsync(request.Username);
        if (exists)
        {
            throw new InvalidOperationException($"El usuario '{request.Username}' ya existe.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            PasswordHash = HashPassword(request.Password),
            Role = "User"
        };

        await _userRepository.AddAsync(user);

        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(2)
        };
    }

    /// <summary>
    /// Autentica un usuario y genera un token JWT.
    /// </summary>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username);
        if (user is null)
        {
            throw new InvalidOperationException("Credenciales inválidas.");
        }

        var passwordHash = HashPassword(request.Password);
        if (user.PasswordHash != passwordHash)
        {
            throw new InvalidOperationException("Credenciales inválidas.");
        }

        var token = _jwtService.GenerateToken(user);

        return new AuthResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(2)
        };
    }

    /// <summary>
    /// Genera un hash SHA256 del password. En producción usar BCrypt.
    /// </summary>
    private static string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }
}
