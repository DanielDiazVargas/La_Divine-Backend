using La_Divine.Src.Dtos.Users;
using La_Divine.Src.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace La_Divine.Src.Controller
{
    /// <summary>
    /// Controlador encargado de gestionar la autenticación de usuarios.
    /// Proporciona los endpoints para el registro de nuevos clientes y el inicio de sesión.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authServices;

        public AuthController(IAuthServices authServices)
        {
            _authServices = authServices;
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// Recibe los datos del usuario, verifica que no existan duplicados y delega la creación al servicio correspondiente.
        /// </summary>
        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                // Resultado de la creación del usuario desde el servicio
                var resultado = await _authServices.RegistrarClienteAsync(createUserDto);

                return Created("", resultado); 
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Inicia sesión en el sistema.
        /// Verifica las credenciales del usuario y, si son válidas, genera y retorna un token JWT.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserDto loginUserDto)
        {
            try
            {
                // Token JWT generado para el usuario autenticado
                var token = await _authServices.IniciarSesionAsync(loginUserDto);
                
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(new { mensaje = ex.Message });
            }
        }
    }
}