using System.Text.Json;
using MasivMovies.Domain.Entities;
using MasivMovies.Domain.Interfaces;
using StackExchange.Redis;

namespace MasivMovies.Infrastructure.Repositories;

/// <summary>
/// Repositorio de salas usando Redis como almacenamiento.
/// </summary>
public sealed class RedisRoomRepository : IRoomRepository
{
    private readonly IDatabase _redis;
    private const string KeyPrefix = "room:";
    private const string IndexKey = "rooms:index";

    public RedisRoomRepository(IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();
    }

    public async Task<Room?> GetByIdAsync(Guid id)
    {
        var data = await _redis.StringGetAsync($"{KeyPrefix}{id}");
        return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<Room>(data!);
    }

    public async Task<IEnumerable<Room>> GetAllAsync()
    {
        var ids = await _redis.SetMembersAsync(IndexKey);
        var rooms = new List<Room>();

        foreach (var id in ids)
        {
            var data = await _redis.StringGetAsync($"{KeyPrefix}{id}");
            if (!data.IsNullOrEmpty)
            {
                var room = JsonSerializer.Deserialize<Room>(data!);
                if (room is not null) rooms.Add(room);
            }
        }

        return rooms;
    }

    public async Task AddAsync(Room room)
    {
        var json = JsonSerializer.Serialize(room);
        await _redis.StringSetAsync($"{KeyPrefix}{room.Id}", json);
        await _redis.SetAddAsync(IndexKey, room.Id.ToString());
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _redis.KeyExistsAsync($"{KeyPrefix}{id}");
    }
}
