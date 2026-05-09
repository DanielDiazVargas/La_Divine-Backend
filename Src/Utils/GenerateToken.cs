using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using La_Divine.Src.Model;

namespace La_Divine.Src.Utils;

/// <summary>
/// Clase utilitaria encargada de generar
/// tokens JWT para los usuarios autenticados.
/// </summary>
public static class GenerateToken {

    /// <summary>
    /// Genera un token JWT utilizando
    /// la información del usuario.
    /// </summary>
    /// <param name="user">
    /// Usuario autenticado.
    /// </param>
    /// <param name="config">
    /// Configuración de la aplicación.
    /// </param>
    /// <returns>
    /// Retorna el token JWT generado.
    /// </returns>
    public static string CreateToken(User user, IConfiguration config) {

        // Clave secreta utilizada para firmar el token
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:key"]!)
        );

        // Credenciales de firma usando HmacSha256
        var creds = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256
        );

        // Información que será almacenada dentro del token
        var claims = new[] {

            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()
            ),

            new Claim(
                ClaimTypes.Email,
                user.Email
            ),

            new Claim(
                ClaimTypes.Name,
                $"{user.Name} {user.LastName}"
            ),

            new Claim(
                ClaimTypes.Role,
                user.Role.Name
            )
        };

        // Creación del token JWT
        var token = new JwtSecurityToken(

            issuer: config["Jwt:issuer"],
            audience: config["Jwt:audience"],

            claims: claims,

            // Tiempo de expiración del token
            expires: DateTime.UtcNow.AddHours(1),

            signingCredentials: creds
        );

        // Retorna el token convertido a string
        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}