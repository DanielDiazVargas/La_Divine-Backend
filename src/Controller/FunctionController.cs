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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromForm] CreateFunction dto)
    {
        var result = await _functionServices.CreateFunctionAsync(dto);
        return Ok(result);
    }
}