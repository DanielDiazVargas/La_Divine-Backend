using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using La_Divine.Src.Db;
using La_Divine.Src.Dtos.Users;
using System.Security.Claims;

namespace La_Divine.Src.Controller
{
    /// <summary>
    /// Controlador encargado de la gestión de usuarios registrados.
    /// Proporciona endpoints para ver perfiles, listar usuarios, cambiar estados y actualizar contraseñas.
    /// Requiere que el usuario esté autenticado en el sistema mediante JWT para acceder.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ContextDb _context;

        public UserController(ContextDb context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene la información del perfil del usuario actualmente autenticado.
        /// Extrae el ID del usuario desde el token JWT.
        /// </summary>
        [HttpGet("perfil")]
        public async Task<IActionResult> GetProfile()
        {
            // Identificador numérico del usuario extraído de los claims del token
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (idClaim == null) return Unauthorized();

            // Usuario buscado en la base de datos incluyendo los datos de su rol
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == int.Parse(idClaim));

            if (user == null) return NotFound();

            return Ok(new UserListDto
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                Email = user.Email,
                Rut = user.Rut,
                Phone = user.Phone,
                Status = user.Status,
                Role = user.Role.Name
            });
        }

        /// <summary>
        /// Obtiene la lista completa de usuarios registrados en el sistema.
        /// Solo accesible por usuarios con el rol de "Admin".
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            // Lista de usuarios proyectada de forma segura hacia el DTO correspondiente
            var users = await _context.Users
                .Include(u => u.Role)
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    LastName = u.LastName,
                    Email = u.Email,
                    Rut = u.Rut,
                    Phone = u.Phone,
                    Status = u.Status,
                    Role = u.Role.Name
                })
                .ToListAsync();

            return Ok(users);
        }

        /// <summary>
        /// Actualiza el estado (habilitado o deshabilitado) de un usuario específico.
        /// Solo accesible por usuarios con el rol de "Admin".
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateUserStatusDto dto)
        {
            // Entidad del usuario a modificar encontrada por su ID
            var user = await _context.Users.FindAsync(id);
            
            if (user == null) return NotFound(new { mensaje = "Usuario no encontrado." });

            user.Status = dto.Status;
            await _context.SaveChangesAsync();

            return Ok(new { user.Id, user.Status });
        }

        /// <summary>
        /// Permite al usuario autenticado cambiar su propia contraseña.
        /// Verifica matemáticamente que la contraseña actual sea correcta antes de aplicar el hash a la nueva.
        /// </summary>
        [HttpPut("cambiar-contrasena")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            // Identificador numérico del usuario extraído de los claims del token
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (idClaim == null) return Unauthorized();

            // Entidad del usuario que solicita el cambio de credenciales
            var user = await _context.Users.FindAsync(int.Parse(idClaim));
            
            if (user == null) return NotFound();

            if (!BCrypt.Net.BCrypt.Verify(dto.Actual, user.Password))
                return BadRequest(new { mensaje = "La contraseña actual es incorrecta." });

            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Nueva);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Contraseña actualizada correctamente." });
        }
    }

    /// <summary>
    /// Objeto de transferencia de datos (DTO) utilizado para procesar la actualización de la contraseña del usuario.
    /// </summary>
    public class ChangePasswordDto
    {
        public string Actual { get; set; } = string.Empty;
        public string Nueva { get; set; } = string.Empty;
    }
}