using System.Text.Json;
using MasivMovies.Domain.Entities;
using MasivMovies.Domain.Interfaces;
using StackExchange.Redis;

namespace MasivMovies.Infrastructure.Repositories;

/// <summary>
/// Repositorio de funciones (showtimes) usando Redis como almacenamiento.
/// </summary>
public sealed class RedisShowtimeRepository : IShowtimeRepository
{
    private readonly IDatabase _redis;
    private const string KeyPrefix = "showtime:";
    private const string IndexKey = "showtimes:index";

    public RedisShowtimeRepository(IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();
    }

    public async Task<Showtime?> GetByIdAsync(Guid id)
    {
        var data = await _redis.StringGetAsync($"{KeyPrefix}{id}");
        return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<Showtime>(data!);
    }

    public async Task<IEnumerable<Showtime>> GetAllAsync()
    {
        var ids = await _redis.SetMembersAsync(IndexKey);
        var showtimes = new List<Showtime>();

        foreach (var id in ids)
        {
            var data = await _redis.StringGetAsync($"{KeyPrefix}{id}");
            if (!data.IsNullOrEmpty)
            {
                var showtime = JsonSerializer.Deserialize<Showtime>(data!);
                if (showtime is not null) showtimes.Add(showtime);
            }
        }

        return showtimes;
    }

    public async Task<IEnumerable<Showtime>> GetByRoomAndDateRangeAsync(Guid roomId, DateTime start, DateTime end)
    {
        var all = await GetAllAsync();
        return all.Where(s =>
            s.RoomId == roomId &&
            s.StartTime < end &&
            s.EndTime > start);
    }

    public async Task AddAsync(Showtime showtime)
    {
        var json = JsonSerializer.Serialize(showtime);
        await _redis.StringSetAsync($"{KeyPrefix}{showtime.Id}", json);
        await _redis.SetAddAsync(IndexKey, showtime.Id.ToString());
    }

    public async Task UpdateAsync(Showtime showtime)
    {
        var json = JsonSerializer.Serialize(showtime);
        await _redis.StringSetAsync($"{KeyPrefix}{showtime.Id}", json);
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _redis.KeyExistsAsync($"{KeyPrefix}{id}");
    }
}
