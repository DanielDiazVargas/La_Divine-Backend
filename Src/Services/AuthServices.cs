using Microsoft.EntityFrameworkCore;
using La_Divine.Src.Model;
using La_Divine.Src.Dtos.Users;
using La_Divine.Src.Services.interfaces;
using La_Divine.Src.Utils;

namespace La_Divine.Src.Services;

/// <summary>
/// Servicio encargado de la autenticación
/// de usuarios.
/// </summary>
public class AuthServices(ContextDb contextDb, IConfiguration config) : IAuthServices {

    /// <summary>
    /// Contexto de la base de datos.
    /// </summary>
    private readonly ContextDb _contextDb = contextDb;

    /// <summary>
    /// Configuración general de la aplicación.
    /// </summary>
    private readonly IConfiguration _config = config;

    /// <summary>
    /// Verifica las credenciales del usuario
    /// y realiza el login.
    /// </summary>
    /// <param name="loginDto">
    /// Datos ingresados por el usuario.
    /// </param>
    /// <returns>
    /// Retorna una respuesta indicando
    /// si el login fue exitoso o no.
    /// </returns>
    public async Task<LoginResponse> Login(LoginDto loginDto) {

        try {

            // Busca el usuario por correo electrónico
            var user = await _contextDb.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            // Verifica si el usuario existe
            // y si la contraseña es correcta
            if(user == null || 
               !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password)) {

                return new LoginResponse {
                    Success = false,
                    Message = "Correo electrónico o contraseña incorrectos",
                    Data = null
                };
            }

            // Verifica si el usuario está habilitado
            if (!user.Status) {

                return new LoginResponse {
                    Success = false,
                    Message = "Usuario inactivo, por favor contacte al administrador",
                    Data = null
                };
            }

            // Retorna los datos del usuario autenticado
            return new LoginResponse {

                Success = true,
                Message = "Login exitoso",

                Data = new DataResponseLogin {

                    // Generación del token JWT
                    Token = GenerateToken.CreateToken(user, _config),

                    UserId = user.Id,
                    Name = user.Name,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role.Name
                }
            };
        }
        catch (Exception ex) {

            // Manejo de errores inesperados
            return new LoginResponse {

                Success = false,
                Message = $"Error durante el login: {ex.Message}",
                Data = null
            };
        }
    }
}