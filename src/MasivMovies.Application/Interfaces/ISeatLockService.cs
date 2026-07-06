namespace MasivMovies.Application.Interfaces;

/// <summary>
/// Contrato para el servicio de bloqueo de asientos usando Redis.
/// Maneja la concurrencia en la compra de asientos y la expiración a 15 minutos.
/// </summary>
public interface ISeatLockService
{
    /// <summary>
    /// Intenta reservar un asiento. Retorna true si se bloqueó exitosamente.
    /// El bloqueo expira automáticamente después de 15 minutos.
    /// </summary>
    Task<bool> TryLockSeatAsync(Guid showtimeId, int seatNumber, Guid userId);

    /// <summary>
    /// Confirma la compra de un asiento previamente bloqueado.
    /// </summary>
    Task<bool> ConfirmSeatAsync(Guid showtimeId, int seatNumber, Guid userId);

    /// <summary>
    /// Libera un asiento reservado.
    /// </summary>
    Task ReleaseSeatAsync(Guid showtimeId, int seatNumber);

    /// <summary>
    /// Obtiene el estado actual de un asiento.
    /// </summary>
    Task<string> GetSeatStatusAsync(Guid showtimeId, int seatNumber);

    /// <summary>
    /// Libera todos los asientos de una función.
    /// </summary>
    Task ReleaseAllSeatsAsync(Guid showtimeId, int totalSeats);
}
