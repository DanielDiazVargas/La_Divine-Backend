namespace La_Divine.Src.Dtos.Seat;

/// <summary>
/// Representa un asiento del teatro para ser mostrado en el mapa de selección.
/// </summary>
public class SeatDto
{
    public int Id { get; set; }
    public string SeatNumber { get; set; } = null!;
    public string Section { get; set; } = null!;
    public decimal Price { get; set; }

    // Estado del asiento: 0 = Disponible, 1 = Reservado
    public int Status { get; set; }
}

/// <summary>
/// Respuesta que contiene la lista de asientos de una función.
/// </summary>
public class GetSeatsResponse
{
    public string Message { get; set; } = null!;
    public bool Success { get; set; }
    public List<SeatDto> Data { get; set; } = [];
}
