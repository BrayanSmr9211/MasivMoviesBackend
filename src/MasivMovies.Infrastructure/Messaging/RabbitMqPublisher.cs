using System.Text;
using System.Text.Json;
using MasivMovies.Application.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace MasivMovies.Infrastructure.Messaging;

/// <summary>
/// Implementación de IMessagePublisher usando RabbitMQ.
/// </summary>
public sealed class RabbitMqPublisher : IMessagePublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IConnection connection, ILogger<RabbitMqPublisher> logger)
    {
        _connection = connection;
        _channel = _connection.CreateModel();
        _logger = logger;
    }

    /// <summary>
    /// Publica un mensaje serializado en JSON a la cola especificada.
    /// </summary>
    public Task PublishAsync<T>(string queueName, T message) where T : class
    {
        _channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        _channel.BasicPublish(
            exchange: string.Empty,
            routingKey: queueName,
            basicProperties: properties,
            body: body);

        _logger.LogInformation("Mensaje publicado en cola '{Queue}'", queueName);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
    }
}
