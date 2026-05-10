namespace La_Divine.Src.Controller;

using La_Divine.Src.Dtos.Function;
using La_Divine.Src.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
// AUN NO IMPLEMENTADA (PARA SIGUIENTES ITERACIONES.)
/// <summary>
/// Controlador encargado de gestionar las obras de teatro.
/// Permite a los administradores crear nuevas funciones y subirlas a la cartelera.
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
        // Objeto que contiene el estado de la creación y los datos de la nueva función
        var result = await _functionServices.CreateFunctionAsync(dto);
        
        return Ok(result);
    }
}