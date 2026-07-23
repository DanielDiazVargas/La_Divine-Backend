namespace La_Divine.Src.Model;

/// <summary>
/// Entidad que representa una reserva realizada por un cliente del teatro.
/// </summary>
public class Reservation
{
    public int Id { get; set; }

    // Código único generado para identificar la reserva, ej: "A3F9C2B1D4E6"
    public string ReservationCode { get; set; } = null!;

    // Usuario que realizó la reserva
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Función asociada a la reserva
    public int FunctionId { get; set; }
    public Function Function { get; set; } = null!;

    // Asientos seleccionados por el cliente (relación N-N con Seat)
    public List<Seat> SelectedSeats { get; set; } = [];

    // Suma total de los precios de los asientos seleccionados
    public decimal TotalPrice { get; set; }

    // Estado de la reserva: 0 = Activa, 1 = Anulada
    public int Status { get; set; } = 0;

    // Estado del pago: false = Pendiente de pago, true = Pagado exitosamente
    public bool IsPaid { get; set; } = false;

    // Fecha y hora de creación de la reserva
    public DateTime CreatedAt { get; set; }
}
