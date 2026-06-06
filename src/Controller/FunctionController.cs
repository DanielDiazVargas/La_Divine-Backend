namespace La_Divine.Src.Controller;

using La_Divine.Src.Dtos.Function;
using La_Divine.Src.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    /// Solo accesible por usuarios con el rol de "Admin".
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromForm] CreateFunction dto)
    {
        var result = await _functionServices.CreateFunctionAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Lista todas las funciones registradas (incluyendo pendientes).
    /// Solo accesible por Admin, para el panel de administración.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllFunctions()
    {
        var functions = await _functionServices.GetAllFunctionsAsync();
        return Ok(functions);
    }

    /// <summary>
    /// Consulta las funciones vigentes en cartelera.
    /// Permite filtrar por nombre y fecha.
    /// Accesible para clientes (User) y administradores (Admin).
    /// </summary>
    [HttpGet("cartelera")]
    [Authorize(Roles = "User,Admin")]
    public async Task<IActionResult> GetBillboard([FromQuery] string? name, [FromQuery] string? date)
    {
        var result = await _functionServices.GetBillboardFunctionsAsync(name, date);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}