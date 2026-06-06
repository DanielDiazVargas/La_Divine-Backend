namespace La_Divine.Src.Controller;

using La_Divine.Src.Dtos.Function;
using La_Divine.Src.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
/// <summary>
/// Controlador encargado de gestionar las funciones teatrales.
/// Permite crear funciones y consultar la cartelera disponible.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FunctionController : ControllerBase
{
    private readonly IFunctionServices _functionServices;

    public FunctionController(IFunctionServices functionServices)
    {
        _functionServices = functionServices;
    }

    /// <summary>
    /// Crea una nueva función en el sistema.
    /// Recibe un formulario con los datos de la obra, incluyendo el archivo de imagen del afiche.
    /// Solo accesible por usuarios con el rol de "Admin".
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromForm] CreateFunction dto)
    {
        // Objeto que contiene el estado de la creación y los datos de la nueva función.
        var result = await _functionServices.CreateFunctionAsync(dto);

        return Ok(result);
    }

    /// <summary>
    /// Consulta las funciones vigentes disponibles en cartelera.
    /// Permite filtrar por nombre de obra y fecha.
    /// Solo accesible para usuarios con rol "User".
    /// </summary>
    [HttpGet("cartelera")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GetBillboard([FromQuery] string? name, [FromQuery] string? date)
    {
        // Solicita al servicio la cartelera filtrada según los criterios recibidos.
        var result = await _functionServices.GetBillboardFunctionsAsync(name, date);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}