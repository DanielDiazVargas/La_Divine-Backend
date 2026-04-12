using Microsoft.AspNetCore.Mvc;
using La_Divine.Src.Dtos.Users;
using La_Divine.Src.Services.interfaces;

namespace La_Divine.Src.Controller;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthServices authServices) : ControllerBase {
    private readonly IAuthServices _authServices = authServices;

    [HttpPost("")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto) {
        try {
            var response = await _authServices.Login(loginDto);
            if (!response.Success) {
                return BadRequest(response);
            }
            return Ok(response);
        }catch(Exception ex) {
            return StatusCode(500, new LoginResponse {
                Success = false,
                Message = $"Error durante el login: {ex.Message}",
                Data = null
            });
        }   
    }    
}