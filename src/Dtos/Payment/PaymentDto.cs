namespace La_Divine.Src.Dtos.Payment;

/// <summary>
/// Datos que envía el frontend para procesar el pago de una reserva con tarjeta de crédito.
/// </summary>
public class ProcessPaymentRequestDto
{
    // Código único de la reserva a pagar (ej: "A1B2C3D4E5F6")
    public string ReservationCode { get; set; } = string.Empty;

    // Número de la tarjeta de crédito (exactamente 16 dígitos)
    public string CardNumber { get; set; } = string.Empty;

    // Código de seguridad de la tarjeta (exactamente 3 dígitos)
    public string SecurityCode { get; set; } = string.Empty;

    // Mes de expiración de la tarjeta (valor entre 1 y 12)
    public string ExpiryMonth { get; set; } = string.Empty;

    // Año de expiración de la tarjeta (4 dígitos, ej: "2027")
    public string ExpiryYear { get; set; } = string.Empty;

    // Nombre y apellido del titular de la tarjeta
    public string CardHolderName { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta que retorna el backend al frontend tras intentar procesar el pago.
/// En caso de éxito se incluyen los datos del movimiento; en caso de rechazo, solo el mensaje de error.
/// </summary>
public class ProcessPaymentResponseDto
{
    // Indica si el pago fue procesado exitosamente
    public bool Success { get; set; }

    // Código de movimiento devuelto por la API de pagos del profesor (solo en caso de éxito)
    public string? TransactionCode { get; set; }

    // Código de la reserva pagada (solo en caso de éxito)
    public string? ReservationCode { get; set; }

    // Monto cobrado en la operación (solo en caso de éxito)
    public decimal? AmountPaid { get; set; }

    // Mensaje de error descriptivo (solo en caso de rechazo o fallo)
    public string? Message { get; set; }
}
