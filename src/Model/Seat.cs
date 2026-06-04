namespace La_Divine.Src.Model;

/// <summary>
/// Entidad que representa un asiento dentro de una función del teatro.
/// </summary>
public class Seat
{
    public int Id { get; set; }

    // Identificador visible del asiento, ej: "A1", "B3"
    public string SeatNumber { get; set; } = null!;

    // Sección a la que pertenece el asiento, ej: VIP Divine, Platea
    public string Section { get; set; } = null!;

    // Precio del asiento según su sección
    public decimal Price { get; set; }

    // Estado del asiento: 0 = Disponible, 1 = Reservado
    public int Status { get; set; } = 0;

    // Función a la que pertenece el asiento
    public int FunctionId { get; set; }
    public Function Function { get; set; } = null!;
}
