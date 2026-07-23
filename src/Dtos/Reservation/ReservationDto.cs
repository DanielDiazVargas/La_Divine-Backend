namespace La_Divine.Src.Dtos.Reservation;

/// <summary>
/// Datos de entrada para crear una reserva desde el frontend.
/// </summary>
public class CreateReservation
{
    public int FunctionId { get; set; }
    public List<int> SeatIds { get; set; } = [];
}

/// <summary>
/// Representa un asiento dentro del detalle de una reserva.
/// </summary>
public class ReservationSeatDto
{
    public int Id { get; set; }
    public string SeatNumber { get; set; } = null!;
    public string Section { get; set; } = null!;
    public decimal Price { get; set; }
}

/// <summary>
/// Datos completos de una reserva procesada y lista para mostrar al cliente.
/// </summary>
public class ReservationData
{
    public int Id { get; set; }
    public string ReservationCode { get; set; } = null!;
    public int FunctionId { get; set; }
    public string FunctionTitle { get; set; } = null!;
    public List<ReservationSeatDto> SelectedSeats { get; set; } = [];
    public decimal TotalPrice { get; set; }

    // Estado de la reserva: 0 = Pagada, 1 = Anulada
    public int Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Respuesta estándar para operaciones de creación y anulación de reservas.
/// </summary>
public class CreateReservationResponse
{
    public string Message { get; set; } = null!;
    public bool Success { get; set; }
    public ReservationData Data { get; set; } = null!;
}

/// <summary>
/// Representa una reserva dentro del historial del cliente.
/// </summary>
public class MyReservationDto
{
    public string ReservationCode { get; set; } = null!;
    public string FunctionTitle { get; set; } = null!;
    public string FunctionDate { get; set; } = null!;
    public string FunctionTime { get; set; } = null!;
    public int SeatCount { get; set; }
    public List<string> SeatNumbers { get; set; } = [];
    public decimal TotalPrice { get; set; }

    // Estado de la reserva: 0 = Activa, 1 = Anulada
    public int Status { get; set; }
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Respuesta con el historial de reservas del cliente autenticado.
/// </summary>
public class MyReservationsResponse
{
    public string Message { get; set; } = null!;
    public bool Success { get; set; }
    public List<MyReservationDto> Data { get; set; } = [];
}
