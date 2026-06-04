using La_Divine.Src.Dtos.Reservation;
using La_Divine.Src.Dtos.Seat;

namespace La_Divine.Src.Services.Interfaces;

/// <summary>
/// Define las operaciones disponibles para la gestión de reservas del teatro.
/// </summary>
public interface IReservationServices
{
    /// <summary>
    /// Obtiene todos los asientos de una función con su estado de disponibilidad.
    /// </summary>
    Task<GetSeatsResponse> GetFunctionSeatsAsync(int functionId);

    /// <summary>
    /// Crea una reserva para el cliente autenticado con los asientos seleccionados.
    /// Utiliza transacción SERIALIZABLE para evitar que dos clientes reserven el mismo asiento.
    /// </summary>
    Task<CreateReservationResponse> CreateReservationAsync(int userId, CreateReservation request);

    /// <summary>
    /// Obtiene el historial de reservas del cliente autenticado.
    /// </summary>
    Task<MyReservationsResponse> GetMyReservationsAsync(int userId);

    /// <summary>
    /// Anula una reserva en estado Pagada y libera los asientos asociados.
    /// </summary>
    Task<CreateReservationResponse> CancelReservationAsync(string reservationCode, int userId);
}
