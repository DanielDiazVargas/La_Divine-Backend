using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using La_Divine.Src.Dtos.Payment;
using La_Divine.Src.Services.Interfaces;

namespace La_Divine.Src.Controller;

/// <summary>
/// Controlador que expone los endpoints de pago para el cliente autenticado.
/// Solo los usuarios con rol "User" (clientes) pueden acceder a estos endpoints (REQ NF02).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "User")]
public class PaymentController(IPaymentService paymentService) : ControllerBase
{
    private readonly IPaymentService _paymentService = paymentService;

    /// <summary>
    /// Procesa el pago de una reserva con tarjeta de crédito llamando a la API del profesor.
    /// Si el cobro es exitoso, marca la reserva como pagada en la base de datos.
    /// POST /api/payment/process
    /// </summary>
    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequestDto dto)
    {
        try
        {
            // Identificador del cliente obtenido desde el token JWT
            var userClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userClaimId, out int userId))
            {
                return Unauthorized(new ProcessPaymentResponseDto
                {
                    Success = false,
                    Message = "Usuario no autenticado."
                });
            }

            var response = await _paymentService.ProcessPaymentAsync(dto, userId);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ProcessPaymentResponseDto
            {
                Success = false,
                Message = $"Error inesperado: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Uso interno — para TAR-002 usar /api/payment/cancel-and-refund.
    /// Procesa el reembolso de una reserva llamando a la API de pagos del profesor.
    /// Solo puede ejecutarse sobre reservas en proceso de anulación (Status = -1) y pagadas.
    /// POST /api/payment/refund
    /// </summary>
    [HttpPost("refund")]
    public async Task<IActionResult> RefundPayment([FromBody] RefundPaymentRequestDto dto)
    {
        try
        {
            // Identificador del cliente obtenido desde el token JWT
            var userClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userClaimId, out int userId))
            {
                return Unauthorized(new RefundPaymentResponseDto
                {
                    Success = false,
                    Message = "Usuario no autenticado."
                });
            }

            var response = await _paymentService.RefundPaymentAsync(dto, userId);
            if (!response.Success) return BadRequest(response);

            return Ok(response);

        }
        catch (Exception ex)
        {
            return StatusCode(500, new RefundPaymentResponseDto
            {
                Success = false,
                Message = $"Error inesperado: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Coordina la anulación de una reserva pagada y su reembolso.
    /// Primero marca la reserva como en proceso de anulación,
    /// luego llama a la API de reembolso. Si falla, revierte la anulación.
    /// POST /api/payment/cancel-and-refund
    /// </summary>
    [HttpPost("cancel-and-refund")]
    public async Task<IActionResult> CancelAndRefund([FromBody] RefundPaymentRequestDto dto, [FromServices] IReservationServices reservationServices)
    {
        try
        {
            // Identificador del cliente obtenido desde el token JWT
            var userClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userClaimId, out int userId))
            {
                return Unauthorized(new RefundPaymentResponseDto
                {
                    Success = false,
                    Message = "Usuario no autenticado."
                });
            }

            // Marcar reserva como en proceso de anulación (Status = -1) y liberar asientos
            var cancelResult = await reservationServices.CancelReservationAsync(dto.ReservationCode, userId);
            if (!cancelResult.Success)
            {
                return BadRequest(new RefundPaymentResponseDto
                {
                    Success = false,
                    Message = cancelResult.Message
                });
            }

            // Llamar a la API de reembolso
            // RefundPaymentAsync verifica que Status == -1 antes de proceder
            var refundResult = await _paymentService.RefundPaymentAsync(dto, userId);
            if (!refundResult.Success)
            {
                // Si el reembolso falla, revertir la anulación
                await reservationServices.RevertCancelReservationAsync(dto.ReservationCode, userId);

                return BadRequest(new RefundPaymentResponseDto
                {
                    Success = false,
                    Message = refundResult.Message
                });
            }

            // Reembolso exitoso, marcar reserva como anulada (Status = 1)
            return Ok(refundResult);
        }
        catch (Exception ex)
        {
            // En caso de error inesperado, intentar revertir la anulación
            var userClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userClaimId, out int userId))
            {
                await reservationServices.RevertCancelReservationAsync(dto.ReservationCode, userId);
            }

            return StatusCode(500, new RefundPaymentResponseDto
            {
                Success = false,
                Message = $"Error inesperado: {ex.Message}"
            });
        }
    }
}