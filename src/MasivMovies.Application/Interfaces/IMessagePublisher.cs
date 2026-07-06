namespace MasivMovies.Application.Interfaces;

/// <summary>
/// Contrato para publicar mensajes en la cola de mensajería.
/// </summary>
public interface IMessagePublisher
{
    Task PublishAsync<T>(string queueName, T message) where T : class;
}
