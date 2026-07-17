using System.Text.Json;
using MasivMovies.Domain.Entities;
using MasivMovies.Domain.Interfaces;
using StackExchange.Redis;

namespace MasivMovies.Infrastructure.Repositories;

/// <summary>
/// Repositorio de usuarios usando Redis como almacenamiento.
/// </summary>
public sealed class RedisUserRepository : IUserRepository
{
    private readonly IDatabase _redis;
    private const string KeyPrefix = "user:";
    private const string UsernameIndexPrefix = "user:username:";
    private const string IndexKey = "users:index";

    public RedisUserRepository(IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        var data = await _redis.StringGetAsync($"{KeyPrefix}{id}");
        return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<User>(data!);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var userId = await _redis.StringGetAsync($"{UsernameIndexPrefix}{username.ToLowerInvariant()}");
        if (userId.IsNullOrEmpty) return null;

        return await GetByIdAsync(Guid.Parse(userId!));
    }

    public async Task AddAsync(User user)
    {
        var json = JsonSerializer.Serialize(user);
        await _redis.StringSetAsync($"{KeyPrefix}{user.Id}", json);
        await _redis.StringSetAsync($"{UsernameIndexPrefix}{user.Username.ToLowerInvariant()}", user.Id.ToString());
        await _redis.SetAddAsync(IndexKey, user.Id.ToString());
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        return await _redis.KeyExistsAsync($"{UsernameIndexPrefix}{username.ToLowerInvariant()}");
    }
}
