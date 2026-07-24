using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using La_Divine.Src.Dtos.Reservation;
using La_Divine.Src.Services.Interfaces;

namespace La_Divine.Src.Controller;

/// <summary>
/// Controlador que expone los endpoints de reservas para el cliente autenticado.
/// Obtiene el identificador del usuario directamente desde el token JWT.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReservationController(IReservationServices reservationServices) : ControllerBase
{
    private readonly IReservationServices _reservationServices = reservationServices;

    /// <summary>
    /// Retorna los asientos de una función con su estado de disponibilidad.
    /// GET /api/reservation/seats/{functionId}
    /// </summary>
    [HttpGet("seats/{functionId}")]
    [Authorize]
    public async Task<IActionResult> GetSeats(int functionId)
    {
        try
        {
            var response = await _reservationServices.GetFunctionSeatsAsync(functionId);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = $"Error inesperado: {ex.Message}" });
        }
    }

    /// <summary>
    /// Crea una reserva para el cliente autenticado con los asientos seleccionados.
    /// POST /api/reservation
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReservation([FromBody] CreateReservation request)
    {
        try
        {
            // Identificador del cliente obtenido desde el token JWT
            var userClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userClaimId, out int userId))
            {
                return Unauthorized(new CreateReservationResponse
                {
                    Success = false,
                    Message = "Usuario no autenticado.",
                    Data = null!
                });
            }

            var response = await _reservationServices.CreateReservationAsync(userId, request);
            if (!response.Success)
                return BadRequest(response);

            return StatusCode(201, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CreateReservationResponse
            {
                Success = false,
                Message = $"Error al crear la reserva: {ex.Message}",
                Data = null!
            });
        }
    }

    /// <summary>
    /// Retorna el historial de reservas del cliente autenticado.
    /// GET /api/reservation/my-reservations
    /// </summary>
    [HttpGet("my-reservations")]
    [Authorize]
    public async Task<IActionResult> GetMyReservations()
    {
        try
        {
            // Identificador del cliente obtenido desde el token JWT
            var userClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userClaimId, out int userId))
            {
                return Unauthorized(new MyReservationsResponse
                {
                    Success = false,
                    Message = "Usuario no autenticado.",
                    Data = []
                });
            }

            var response = await _reservationServices.GetMyReservationsAsync(userId);
            if (!response.Success)
                return NotFound(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new MyReservationsResponse
            {
                Success = false,
                Message = $"Error inesperado: {ex.Message}",
                Data = []
            });
        }
    }

    /// <summary>
    /// Anula una reserva del cliente autenticado y libera los asientos asociados.
    /// PATCH /api/reservation/{code}/cancel
    /// </summary>
    [HttpPatch("{code}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelReservation(string code)
    {
        try
        {
            // Identificador del cliente obtenido desde el token JWT
            var userClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userClaimId, out int userId))
            {
                return Unauthorized(new CreateReservationResponse
                {
                    Success = false,
                    Message = "Usuario no autenticado.",
                    Data = null!
                });
            }

            var response = await _reservationServices.CancelReservationAsync(code, userId);
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new CreateReservationResponse
            {
                Success = false,
                Message = $"Error al anular la reserva: {ex.Message}",
                Data = null!
            });
        }
    }
    /// <summary>
    /// Descarga la entrada en PDF de una reserva pagada del cliente autenticado.
    /// GET /api/reservation/{code}/pdf
    /// </summary>
    [HttpGet("{code}/pdf")]
    [Authorize]
    public async Task<IActionResult> DownloadPdf(string code)
    {
        try
        {
            var userClaimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userClaimId, out int userId))
            {
                return Unauthorized(new PdfReservationResponse
                {
                    Success = false,
                    Message = "Usuario no autenticado.",
                    Data = []
                });
            }

            var response = await _reservationServices.GetReservationPdfAsync(userId, code);
            if (!response.Success)
                return BadRequest(response);

            return File(response.Data, "application/pdf", $"entrada_{code}.pdf");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new PdfReservationResponse
            {
                Success = false,
                Message = $"Error al generar el PDF: {ex.Message}",
                Data = []
            });
        }
    }
}
