using System.Text.Json;
using MasivMovies.Domain.Entities;
using MasivMovies.Domain.Interfaces;
using StackExchange.Redis;

namespace MasivMovies.Infrastructure.Repositories;

/// <summary>
/// Repositorio de boletos usando Redis como almacenamiento.
/// </summary>
public sealed class RedisTicketRepository : ITicketRepository
{
    private readonly IDatabase _redis;
    private const string KeyPrefix = "ticket:";
    private const string IndexKey = "tickets:index";
    private const string ShowtimeIndexPrefix = "tickets:showtime:";

    public RedisTicketRepository(IConnectionMultiplexer connectionMultiplexer)
    {
        _redis = connectionMultiplexer.GetDatabase();
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
    {
        var data = await _redis.StringGetAsync($"{KeyPrefix}{id}");
        return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<Ticket>(data!);
    }

    public async Task<IEnumerable<Ticket>> GetByShowtimeIdAsync(Guid showtimeId)
    {
        var ids = await _redis.SetMembersAsync($"{ShowtimeIndexPrefix}{showtimeId}");
        var tickets = new List<Ticket>();

        foreach (var id in ids)
        {
            var data = await _redis.StringGetAsync($"{KeyPrefix}{id}");
            if (!data.IsNullOrEmpty)
            {
                var ticket = JsonSerializer.Deserialize<Ticket>(data!);
                if (ticket is not null) tickets.Add(ticket);
            }
        }

        return tickets;
    }

    public async Task AddAsync(Ticket ticket)
    {
        var json = JsonSerializer.Serialize(ticket);
        await _redis.StringSetAsync($"{KeyPrefix}{ticket.Id}", json);
        await _redis.SetAddAsync(IndexKey, ticket.Id.ToString());
        await _redis.SetAddAsync($"{ShowtimeIndexPrefix}{ticket.ShowtimeId}", ticket.Id.ToString());
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        var json = JsonSerializer.Serialize(ticket);
        await _redis.StringSetAsync($"{KeyPrefix}{ticket.Id}", json);
    }

    public async Task DeleteByShowtimeIdAsync(Guid showtimeId)
    {
        var ids = await _redis.SetMembersAsync($"{ShowtimeIndexPrefix}{showtimeId}");
        foreach (var id in ids)
        {
            await _redis.KeyDeleteAsync($"{KeyPrefix}{id}");
            await _redis.SetRemoveAsync(IndexKey, id);
        }
        await _redis.KeyDeleteAsync($"{ShowtimeIndexPrefix}{showtimeId}");
    }

    public async Task<int> GetSoldTicketsCountByShowtimeAsync(Guid showtimeId)
    {
        var tickets = await GetByShowtimeIdAsync(showtimeId);
        return tickets.Count(t => t.Status == TicketStatus.Sold);
    }

    public async Task<IEnumerable<(Guid ShowtimeId, int TicketsSold)>> GetMonthlyReportAsync(int year, int month)
    {
        var allTicketIds = await _redis.SetMembersAsync(IndexKey);
        var showtimeTickets = new Dictionary<Guid, int>();

        foreach (var ticketId in allTicketIds)
        {
            var data = await _redis.StringGetAsync($"{KeyPrefix}{ticketId}");
            if (data.IsNullOrEmpty) continue;

            var ticket = JsonSerializer.Deserialize<Ticket>(data!);
            if (ticket is null || ticket.Status != TicketStatus.Sold) continue;
            if (ticket.ConfirmedAt is null) continue;
            if (ticket.ConfirmedAt.Value.Year != year || ticket.ConfirmedAt.Value.Month != month) continue;

            if (!showtimeTickets.ContainsKey(ticket.ShowtimeId))
            {
                showtimeTickets[ticket.ShowtimeId] = 0;
            }
            showtimeTickets[ticket.ShowtimeId]++;
        }

        return showtimeTickets
            .Select(kvp => (kvp.Key, kvp.Value))
            .OrderByDescending(x => x.Value);
    }
}
