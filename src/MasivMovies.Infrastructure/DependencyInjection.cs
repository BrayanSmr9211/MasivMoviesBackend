using MasivMovies.Application.Interfaces;
using MasivMovies.Application.Services;
using MasivMovies.Domain.Interfaces;
using MasivMovies.Infrastructure.Auth;
using MasivMovies.Infrastructure.Cache;
using MasivMovies.Infrastructure.Messaging;
using MasivMovies.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace MasivMovies.Infrastructure;

/// <summary>
/// Extensión para registrar las dependencias de infraestructura.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Redis
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnection));

        // RabbitMQ
        var rabbitHost = configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
        var rabbitPort = configuration.GetValue<int>("RabbitMQ:Port");
        if (rabbitPort == 0) rabbitPort = 5672;
        var rabbitUser = configuration.GetValue<string>("RabbitMQ:Username") ?? "guest";
        var rabbitPassword = configuration.GetValue<string>("RabbitMQ:Password") ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = rabbitHost,
            Port = rabbitPort,
            UserName = rabbitUser,
            Password = rabbitPassword
        };

        services.AddSingleton<IConnection>(factory.CreateConnection());

        // Repositorios
        services.AddSingleton<IMovieRepository, RedisMovieRepository>();
        services.AddSingleton<IRoomRepository, RedisRoomRepository>();
        services.AddSingleton<IShowtimeRepository, RedisShowtimeRepository>();
        services.AddSingleton<ITicketRepository, RedisTicketRepository>();
        services.AddSingleton<IUserRepository, RedisUserRepository>();

        // Servicios de infraestructura
        services.AddSingleton<ISeatLockService, RedisSeatLockService>();
        services.AddSingleton<IMessagePublisher, RabbitMqPublisher>();
        services.AddSingleton<IJwtService, JwtService>();

        // Servicios de aplicación
        services.AddScoped<MovieService>();
        services.AddScoped<RoomService>();
        services.AddScoped<ShowtimeService>();
        services.AddScoped<TicketService>();
        services.AddScoped<ReportService>();
        services.AddScoped<AuthService>();

        return services;
    }
}
