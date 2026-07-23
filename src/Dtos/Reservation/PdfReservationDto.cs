namespace La_Divine.Src.Dtos.Reservation;

/// <summary>
/// Datos de una reserva necesarios para generar su entrada en PDF (FUN-005).
/// </summary>
public class PdfReservationDto
{
    public string ReservationCode { get; set; } = null!;
    public string FunctionTitle { get; set; } = null!;
    public string FunctionDate { get; set; } = null!;
    public string FunctionTime { get; set; } = null!;
    public string HolderName { get; set; } = null!;
    public List<ReservationSeatDto> SelectedSeats { get; set; } = [];
    public decimal TotalPrice { get; set; }
}

/// <summary>
/// Respuesta del servicio de generación de PDF: mensaje, éxito y el archivo en bytes.
/// </summary>
public class PdfReservationResponse
{
    public string Message { get; set; } = null!;
    public bool Success { get; set; }
    public byte[] Data { get; set; } = [];
}