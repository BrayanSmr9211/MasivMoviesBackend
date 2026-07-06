using System.Text.Json;
using MasivMovies.Domain.Entities;
using MasivMovies.Domain.Interfaces;
using StackExchange.Redis;

namespace MasivMovies.Infrastructure.Repositories;

/// <summary>
/// Repositorio de películas usando Redis como almacenamiento.
/// </summary>
public sealed class RedisMovieRepository : IMovieRepository
{
    private readonly IDatabase _redis;
    private const string KeyPrefix = "movie:";
    private const string IndexKey = "movies:index";

    public RedisMovieRepository(IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        var data = await _redis.StringGetAsync($"{KeyPrefix}{id}");
        return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<Movie>(data!);
    }

    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        var ids = await _redis.SetMembersAsync(IndexKey);
        var movies = new List<Movie>();

        foreach (var id in ids)
        {
            var data = await _redis.StringGetAsync($"{KeyPrefix}{id}");
            if (!data.IsNullOrEmpty)
            {
                var movie = JsonSerializer.Deserialize<Movie>(data!);
                if (movie is not null) movies.Add(movie);
            }
        }

        return movies;
    }

    public async Task AddAsync(Movie movie)
    {
        var json = JsonSerializer.Serialize(movie);
        await _redis.StringSetAsync($"{KeyPrefix}{movie.Id}", json);
        await _redis.SetAddAsync(IndexKey, movie.Id.ToString());
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _redis.KeyExistsAsync($"{KeyPrefix}{id}");
    }
}
