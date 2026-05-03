using La_Divine.Src.Dtos.Users;
using La_Divine.Src.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace La_Divine.Src.Controller
{
    // 1. Configuramos la ruta base: http://localhost:puerto/api/Auth
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authServices;

        // Inyectamos el servicio
        public AuthController(IAuthServices authServices)
        {
            _authServices = authServices;
        }

        // 2. Ruta para Registrar Cliente (Método POST)
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                // Le pasamos los datos al servicio
                var resultado = await _authServices.RegistrarClienteAsync(createUserDto);

                return Created("", resultado); 
            }
            catch (Exception ex)
            {
                // Si salta un error, devolvemos un Bad Request
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        // 3. Ruta para Iniciar Sesión (Método POST)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            try
            {
                var token = await _authServices.IniciarSesionAsync(loginUserDto);
                // Si el login es exitoso, devolvemos el token
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
        }
    }
}