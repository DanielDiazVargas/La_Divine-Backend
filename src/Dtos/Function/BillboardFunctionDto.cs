namespace La_Divine.Src.Dtos.Function;

/// <summary>
/// DTO utilizado para enviar al frontend la información necesaria
/// para mostrar una función vigente en la cartelera.
/// </summary>
public class BillboardFunctionDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string DateFunction { get; set; } = string.Empty;

    public string TimeFunction { get; set; } = string.Empty;

    public int AvailableSeats { get; set; }

    public int TotalSeats { get; set; }

    // Precio mínimo de los asientos generados para la función
    public decimal BasePrice { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta estándar utilizada por el endpoint de consulta de cartelera.
/// </summary>
public class BillboardFunctionResponse
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public List<BillboardFunctionDto> Data { get; set; } = [];
}