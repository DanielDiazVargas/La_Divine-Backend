namespace La_Divine.Src.Controller;

using La_Divine.Src.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Controlador que expone los endpoints de reportes administrativos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ReportController(IReportServices reportServices) : ControllerBase
{
    private readonly IReportServices _reportServices = reportServices;

    /// <summary>
    /// Obtiene el reporte de ventas para las funciones dentro de un rango de fechas.
    /// GET /api/report/sales?startDate=2026-07-01&amp;endDate=2026-07-10
    /// </summary>
    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesReport(
        [FromQuery] string? startDate,
        [FromQuery] string? endDate)
    {
        var result = await _reportServices.GetSalesReportAsync(startDate, endDate);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}