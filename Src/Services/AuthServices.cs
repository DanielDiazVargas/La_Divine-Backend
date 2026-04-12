using Microsoft.EntityFrameworkCore;
using La_Divine.Src.Model;
using La_Divine.Src.Dtos.Users;
using La_Divine.Src.Services.interfaces;
using La_Divine.Src.Utils;

namespace La_Divine.Src.Services;

public class AuthServices(ContextDb contextDb, IConfiguration config) : IAuthServices
{
    private readonly ContextDb _contextDb = contextDb;
    private readonly IConfiguration _config = config;

    public async Task<LoginResponse> Login(LoginDto loginDto)
    {
        try {   
            var user = await _contextDb.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u=> u.Email == loginDto.Email);

            // Verificación de las credenciales del usuario. Si el usuario no existe o la contraseña no coincide, se devuelve una respuesta de error.
            if(user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password)) {
                // Retorno de una respuesta indicando que el correo electrónico o la contraseña son incorrectos.
                return new LoginResponse {
                    Success = false,
                    Message = "Correo electrónico o contraseña incorrectos",
                    Data = null
                };
            }

            // Verificación del estado del usuario. Si el usuario está inactivo, se devuelve una respuesta indicando que el usuario está inactivo y que debe contactar al administrador.
            if (!user.Status) {
                // Retorno de una respuesta indicando que el usuario está inactivo y que debe contactar al administrador.
                return new LoginResponse  {
                    Success = false,
                    Message = "Usuario inactivo, por favor contacte al administrador",
                    Data = null
                };
            }

            user.Name = "Francisco2 ";

            await _contextDb.SaveChangesAsync();
            return new LoginResponse {
                Success = true,
                Message = "Login exitoso",
                Data = new DataResponseLogin {
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
            return new LoginResponse {
                Success = false,
                Message = $"Error durante el login: {ex.Message}",
                Data = null
            };
        }
    }
}