using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MasivMovies.Application.Interfaces;
using MasivMovies.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MasivMovies.Infrastructure.Auth;

/// <summary>
/// Implementación de IJwtService para generar tokens JWT.
/// </summary>
public sealed class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Genera un token JWT con claims de usuario, con expiración de 2 horas.
    /// </summary>
    public string GenerateToken(User user)
    {
        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException("Jwt:SecretKey no está configurado.");

        var issuer = _configuration["Jwt:Issuer"] ?? "MasivMovies";
        var audience = _configuration["Jwt:Audience"] ?? "MasivMoviesAPI";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("sub", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
