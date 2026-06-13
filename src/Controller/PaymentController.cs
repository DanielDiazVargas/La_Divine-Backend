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
}
