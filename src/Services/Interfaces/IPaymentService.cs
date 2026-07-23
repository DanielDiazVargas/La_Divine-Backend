using La_Divine.Src.Dtos.Payment;

namespace La_Divine.Src.Services.Interfaces;

/// <summary>
/// Contrato del servicio de procesamiento de pagos con tarjeta de crédito.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Procesa el pago de una reserva llamando a la API externa del profesor.
    /// Actualiza IsPaid en la BD si el cobro es exitoso.
    /// Retorna un objeto con el resultado del movimiento o un mensaje de error.
    /// </summary>
    Task<ProcessPaymentResponseDto> ProcessPaymentAsync(ProcessPaymentRequestDto dto, int userId);
}
