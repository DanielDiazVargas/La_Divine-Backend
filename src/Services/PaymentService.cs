using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using La_Divine.Src.Db;
using La_Divine.Src.Dtos.Payment;
using La_Divine.Src.Services.Interfaces;

namespace La_Divine.Src.Services;

/// <summary>
/// Servicio que gestiona el procesamiento de pagos con tarjeta de crédito.
/// Se comunica con la API de pagos del profesor para registrar el cobro,
/// y actualiza el estado de la reserva en la base de datos si el cobro es exitoso.
/// </summary>
public class PaymentService(ContextDb contextDb, HttpClient httpClient, ISendGridEmailServices sendGridEmailServices) : IPaymentService
{
    private readonly ContextDb _contextDb = contextDb;
    private readonly HttpClient _httpClient = httpClient;
    private readonly ISendGridEmailServices _sendGridEmailServices = sendGridEmailServices;

    // Mensaje de error estándar definido en la ERS para pagos rechazados o fallidos
    private const string PaymentRejectedMessage = "El pago no pudo ser procesado. Verifique los datos ingresados e intente nuevamente";

    // Mensaje de error estándar definido en la ERS para reservas que no pueden ser anuladas
    private const string RefundRejectedMessage = "Esta reserva no puede ser anulada";

    /// <summary>
    /// Procesa el pago de una reserva llamando a la API externa del profesor.
    /// Actualiza IsPaid en la BD si el cobro es exitoso.
    /// Retorna un objeto con el resultado del movimiento o un mensaje de error estándar.
    /// </summary>
    public async Task<ProcessPaymentResponseDto> ProcessPaymentAsync(ProcessPaymentRequestDto dto, int userId)
    {
        try
        {
            // Validación de los campos de la tarjeta de crédito antes de consultar la BD
            var validationError = ValidateCardData(dto);
            if (validationError != null)
            {
                return new ProcessPaymentResponseDto
                {
                    Success = false,
                    Message = validationError
                };
            }

            // Búsqueda de la reserva incluyendo al usuario para obtener su email (requerido por la API del profesor)
            var reservation = await _contextDb.Reservations
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReservationCode == dto.ReservationCode && r.UserId == userId);

            if (reservation == null)
            {
                return new ProcessPaymentResponseDto
                {
                    Success = false,
                    Message = "Reserva no encontrada o no pertenece al usuario autenticado."
                };
            }

            // Verificación de que la reserva aún no ha sido pagada
            if (reservation.IsPaid)
            {
                return new ProcessPaymentResponseDto
                {
                    Success = false,
                    Message = "Esta reserva ya fue pagada anteriormente."
                };
            }

            // Construcción del payload que espera la API de pagos del profesor
            var paymentPayload = new
            {
                numero_tarjeta = dto.CardNumber,
                codigo_seguridad = dto.SecurityCode,
                mes_expiracion = dto.ExpiryMonth,
                anio_expiracion = dto.ExpiryYear,
                nombre_titular = dto.CardHolderName,
                monto = reservation.TotalPrice,
                codigo_reserva = reservation.ReservationCode,
                email_cliente = reservation.User.Email
            };

            // Serialización del payload a JSON para el cuerpo de la solicitud HTTP
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(paymentPayload),
                Encoding.UTF8,
                "application/json"
            );

            // Envío de la solicitud a la API de pagos del profesor con el timeout configurado en Program.cs (REQ NF07)
            var response = await _httpClient.PostAsync("/pago/procesar", jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                return new ProcessPaymentResponseDto
                {
                    Success = false,
                    Message = PaymentRejectedMessage
                };
            }

            // Deserialización de la respuesta exitosa de la API del profesor
            var responseBody = await response.Content.ReadAsStringAsync();
            var externalResponse = JsonSerializer.Deserialize<ExternalPaymentResponse>(responseBody);

            if (externalResponse == null)
            {
                return new ProcessPaymentResponseDto
                {
                    Success = false,
                    Message = PaymentRejectedMessage
                };
            }

            // Actualización del estado de pago de la reserva en la base de datos
            reservation.IsPaid = true;
            await _contextDb.SaveChangesAsync();

            return new ProcessPaymentResponseDto
            {
                Success = true,
                TransactionCode = externalResponse.CodigoMovimiento.ToString(),
                ReservationCode = externalResponse.CodigoReserva,
                AmountPaid = externalResponse.Monto
            };
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error al conectar con la API de pagos: {ex.Message}");
            return new ProcessPaymentResponseDto
            {
                Success = false,
                Message = PaymentRejectedMessage
            };
        }
        catch (TaskCanceledException)
        {
            // Se produce cuando la solicitud supera el timeout de 10 segundos configurado (REQ NF07)
            Console.WriteLine("La solicitud a la API de pagos superó el tiempo límite.");
            return new ProcessPaymentResponseDto
            {
                Success = false,
                Message = PaymentRejectedMessage
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado al procesar el pago: {ex.Message}");
            return new ProcessPaymentResponseDto
            {
                Success = false,
                Message = PaymentRejectedMessage
            };
        }
    }

    /// <summary>
    /// Valida el formato de los campos de la tarjeta de crédito.
    /// Retorna el primer mensaje de error encontrado, o null si todos los campos son válidos.
    /// </summary>
    private static string? ValidateCardData(ProcessPaymentRequestDto dto)
    {
        // El número de tarjeta debe tener exactamente 16 dígitos numéricos
        if (string.IsNullOrWhiteSpace(dto.CardNumber) || dto.CardNumber.Length != 16 || !dto.CardNumber.All(char.IsDigit))
            return "El número de tarjeta debe tener exactamente 16 dígitos numéricos.";

        // El código de seguridad debe tener exactamente 3 dígitos numéricos
        if (string.IsNullOrWhiteSpace(dto.SecurityCode) || dto.SecurityCode.Length != 3 || !dto.SecurityCode.All(char.IsDigit))
            return "El código de seguridad debe tener exactamente 3 dígitos numéricos.";

        // El mes de expiración debe ser un número entre 1 y 12
        if (!int.TryParse(dto.ExpiryMonth, out int month) || month < 1 || month > 12)
            return "El mes de expiración debe ser un número entre 1 y 12.";

        // El año de expiración debe ser un número de 4 dígitos y no puede ser anterior al año actual
        if (!int.TryParse(dto.ExpiryYear, out int year) || dto.ExpiryYear.Length != 4 || year < DateTime.UtcNow.Year)
            return "El año de expiración debe tener 4 dígitos y no puede ser anterior al año actual.";

        // El nombre del titular no puede estar vacío
        if (string.IsNullOrWhiteSpace(dto.CardHolderName))
            return "El nombre del titular de la tarjeta es requerido.";

        return null;
    }

    /// <summary>
    /// Clase interna para deserializar la respuesta exitosa de la API de pagos del profesor.
    /// Los campos usan snake_case porque así los retorna la API externa.
    /// Nota: codigo_movimiento llega como string y el campo de monto se llama "monto" (no "monto_cobrado").
    /// </summary>
    private class ExternalPaymentResponse
    {
        [JsonPropertyName("codigo_movimiento")]
        public int CodigoMovimiento { get; set; }

        [JsonPropertyName("codigo_reserva")]
        public string CodigoReserva { get; set; } = string.Empty;

        [JsonPropertyName("monto")]
        public decimal Monto { get; set; }
    }

    /// <summary>
    /// Clase interna para deserializar la respuesta exitosa de /pago/anular en la API del profesor.
    /// A diferencia de /pago/procesar, el monto se devuelve en el campo "monto_reembolsado".
    /// </summary>
    private class ExternalRefundResponse
    {
        [JsonPropertyName("codigo_movimiento")]
        public int CodigoMovimiento { get; set; }

        [JsonPropertyName("codigo_reserva")]
        public string CodigoReserva { get; set; } = string.Empty;

        [JsonPropertyName("monto_reembolsado")]
        public decimal MontoReembolsado { get; set; }
    }

    /// <summary>
    /// Clase interna para deserializar los mensajes de error que retorna la API del profesor
    /// cuando la solicitud es rechazada (ej: "Esta reserva ya fue reembolsada.").
    /// </summary>
    private class ExternalErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    /// <summary>
    /// Procesa el reembolzo de una reserva llamando a la API externa del profesor.
    /// Actualiza Status en la BD si el reembolzo es exitoso.
    /// Retorna un objeto con el resultado del movimiento o un mensaje de error estándar.
    /// </summary>
    public async Task<RefundPaymentResponseDto> RefundPaymentAsync(RefundPaymentRequestDto dto, int userId)
    {
        try
        {
            // Búsqueda de la reserva incluyendo al usuario para obtener su email (requerido por la API del profesor)
            var reservation = await _contextDb.Reservations
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReservationCode == dto.ReservationCode && r.UserId == userId);

            if (reservation == null)
            {
                return new RefundPaymentResponseDto
                {
                    Success = false,
                    Message = "Reserva no encontrada o no pertenece al usuario autenticado."
                };
            }

            // Verificación si la reserva no ha sido pagado o este proceso de anulación
            if (!reservation.IsPaid || reservation.Status != -1)
            {
                return new RefundPaymentResponseDto
                {
                    Success = false,
                    Message = RefundRejectedMessage
                };
            }

            // Construcción del payload que espera la API de pagos del profesor
            var refundPayload = new
            {
                codigo_reserva = reservation.ReservationCode,
                email_cliente = reservation.User.Email
            };

            // Serialización del payload a JSON para el cuerpo de la solicitud HTTP
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(refundPayload),
                Encoding.UTF8,
                "application/json"
            );

            // Envío de la solicitud a la API de pagos del profesor con el timeout configurado en Program.cs (REQ NF07)
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsync("/pago/anular", jsonContent);
            }
            catch (TaskCanceledException)
            {
                // El timeout del cliente no cancela el procesamiento en el servidor de la API del
                // profesor (corre en hosting gratuito que puede tardar en "despertar" tras
                // inactividad). Se reintenta una vez para averiguar si la operación terminó
                // completándose igual, en vez de asumir que falló y revertir localmente una
                // anulación que en realidad sí ocurrió del lado del proveedor de pagos.
                var retryContent = new StringContent(
                    JsonSerializer.Serialize(refundPayload),
                    Encoding.UTF8,
                    "application/json"
                );
                response = await _httpClient.PostAsync("/pago/anular", retryContent);
            }

            var alreadyRefunded = false;

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                var externalError = JsonSerializer.Deserialize<ExternalErrorResponse>(errorBody);

                // Si la API indica que la reserva ya fue reembolsada, es porque el intento original
                // (el que agotó el timeout) sí se había completado en el servidor. Se continúa como
                // éxito para no dejar la reserva desincronizada entre nuestra BD y el sistema de pagos.
                if (externalError?.Error != null &&
                    externalError.Error.Contains("ya fue reembolsada", StringComparison.OrdinalIgnoreCase))
                {
                    alreadyRefunded = true;
                }
                else
                {
                    return new RefundPaymentResponseDto
                    {
                        Success = false,
                        Message = RefundRejectedMessage
                    };
                }
            }

            if (!alreadyRefunded)
            {
                // Deserialización de la respuesta exitosa de la API del profesor
                var responseBody = await response.Content.ReadAsStringAsync();
                var externalResponse = JsonSerializer.Deserialize<ExternalRefundResponse>(responseBody);

                if (externalResponse == null)
                {
                    return new RefundPaymentResponseDto
                    {
                        Success = false,
                        Message = RefundRejectedMessage
                    };
                }
            }

            reservation.Status = 1; // Anulada
            reservation.IsPaid = false;
            await _contextDb.SaveChangesAsync();

            // Notificación al cliente informando la anulación y el reembolso (TAR-002)
            await _sendGridEmailServices.SendRefundEmailAsync(
                toEmail: reservation.User.Email,
                nombre: reservation.User.Name,
                codigoReserva: reservation.ReservationCode,
                montoReembolsado: reservation.TotalPrice.ToString("N0")
            );

            return new RefundPaymentResponseDto
            {
                Success = true,
                Message = "Reservación reembolsada exitosamente"
            };
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Error al conectar con la API de pagos: {ex.Message}");
            return new RefundPaymentResponseDto
            {
                Success = false,
                Message = PaymentRejectedMessage
            };
        }
        catch (TaskCanceledException)
        {
            // Se produce cuando la solicitud supera el timeout de 10 segundos configurado (REQ NF07)
            Console.WriteLine("La solicitud a la API de pagos superó el tiempo límite.");
            return new RefundPaymentResponseDto
            {
                Success = false,
                Message = PaymentRejectedMessage
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error inesperado al procesar el pago: {ex.Message}");
            return new RefundPaymentResponseDto
            {
                Success = false,
                Message = PaymentRejectedMessage
            };
        }
    }
}
