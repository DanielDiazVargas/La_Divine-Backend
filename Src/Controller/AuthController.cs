using Microsoft.AspNetCore.Mvc;
using La_Divine.Src.Dtos.Users;
using La_Divine.Src.Services.interfaces;

namespace La_Divine.Src.Controller;

/// <summary>
/// Controlador encargado del login de usuarios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthServices authServices) : ControllerBase {

    /// <summary>
    /// Servicio utilizado para la autenticación.
    /// </summary>
    private readonly IAuthServices _authServices = authServices;

    /// <summary>
    /// Permite iniciar sesión usando correo y contraseña.
    /// </summary>
    /// <param name="loginDto">
    /// Datos ingresados por el usuario para el login.
    /// </param>
    /// <returns>
    /// Retorna un OK si el login es correcto,
    /// un BadRequest si las credenciales son incorrectas
    /// o un error 500 si ocurre un problema inesperado.
    /// </returns>
    [HttpPost("")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto) {

        try {

            // Se llama al servicio de autenticación
            var response = await _authServices.Login(loginDto);

            // Si el login falla, retorna un BadRequest
            if (!response.Success) {
                return BadRequest(response);
            }

            // Login exitoso
            return Ok(response);

        } catch(Exception ex) {

            // Manejo de errores inesperados
            return StatusCode(500, new LoginResponse {
                Success = false,
                Message = $"Error durante el login: {ex.Message}",
                Data = null
            });
        }   
    }    
}