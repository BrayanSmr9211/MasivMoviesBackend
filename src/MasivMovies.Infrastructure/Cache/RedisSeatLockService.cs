using MasivMovies.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace MasivMovies.Infrastructure.Cache;

/// <summary>
/// Implementación de ISeatLockService usando Redis.
/// Maneja bloqueos atómicos con expiración de 15 minutos para la concurrencia de asientos.
/// </summary>
public sealed class RedisSeatLockService : ISeatLockService
{
    private readonly IDatabase _redis;
    private readonly ILogger<RedisSeatLockService> _logger;
    private static readonly TimeSpan LockExpiry = TimeSpan.FromMinutes(15);

    public RedisSeatLockService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisSeatLockService> logger)
    {
        _redis = connectionMultiplexer.GetDatabase();
        _logger = logger;
    }

    /// <summary>
    /// Intenta reservar un asiento usando SET NX (atómico) con TTL de 15 minutos.
    /// </summary>
    public async Task<bool> TryLockSeatAsync(Guid showtimeId, int seatNumber, Guid userId)
    {
        var key = GetSeatKey(showtimeId, seatNumber);
        var value = $"reserved:{userId}";

        var acquired = await _redis.StringSetAsync(key, value, LockExpiry, When.NotExists);

        if (acquired)
        {
            _logger.LogInformation(
                "Asiento {Seat} reservado para función {Showtime} por usuario {User}",
                seatNumber, showtimeId, userId);
        }

        return acquired;
    }

    /// <summary>
    /// Confirma la compra cambiando el valor a "sold" y eliminando el TTL.
    /// </summary>
    public async Task<bool> ConfirmSeatAsync(Guid showtimeId, int seatNumber, Guid userId)
    {
        var key = GetSeatKey(showtimeId, seatNumber);
        var currentValue = await _redis.StringGetAsync(key);

        if (currentValue.IsNullOrEmpty)
        {
            return false;
        }

        var expectedValue = $"reserved:{userId}";
        if (currentValue.ToString() != expectedValue)
        {
            return false;
        }

        var soldValue = $"sold:{userId}";
        await _redis.StringSetAsync(key, soldValue);
        await _redis.KeyPersistAsync(key);

        _logger.LogInformation(
            "Asiento {Seat} confirmado para función {Showtime}",
            seatNumber, showtimeId);

        return true;
    }

    /// <summary>
    /// Libera un asiento eliminando la key en Redis.
    /// </summary>
    public async Task ReleaseSeatAsync(Guid showtimeId, int seatNumber)
    {
        var key = GetSeatKey(showtimeId, seatNumber);
        await _redis.KeyDeleteAsync(key);
    }

    /// <summary>
    /// Obtiene el estado actual de un asiento.
    /// </summary>
    public async Task<string> GetSeatStatusAsync(Guid showtimeId, int seatNumber)
    {
        var key = GetSeatKey(showtimeId, seatNumber);
        var value = await _redis.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            return "Available";
        }

        var strValue = value.ToString();
        if (strValue.StartsWith("sold:"))
        {
            return "Sold";
        }

        return "Reserved";
    }

    /// <summary>
    /// Libera todos los asientos de una función.
    /// </summary>
    public async Task ReleaseAllSeatsAsync(Guid showtimeId, int totalSeats)
    {
        for (int i = 1; i <= totalSeats; i++)
        {
            await ReleaseSeatAsync(showtimeId, i);
        }

        _logger.LogInformation(
            "Todos los asientos liberados para función {Showtime}", showtimeId);
    }

    private static string GetSeatKey(Guid showtimeId, int seatNumber)
    {
        return $"seat:{showtimeId}:{seatNumber}";
    }
}
