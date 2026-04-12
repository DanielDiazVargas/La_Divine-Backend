using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using La_Divine.Src.Model;

namespace La_Divine.Src.Utils;

public static class GenerateToken {

    public static string CreateToken(User user, IConfiguration config) {
        // Creación de la clave de seguridad simétrica utilizando la clave definida en la configuración de la aplicación.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:key"]!) );

        // Creación de las credenciales de firma utilizando la clave de seguridad y el algoritmo HMAC SHA256.
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Definición de las reclamaciones que se incluirán en el token JWT, representando la información del usuario.
        var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.Name} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role.Name)
        };

        // Creación del token JWT utilizando las reclamaciones, las credenciales de firma, y la configuración de emisor, audiencia y expiración.
        var token = new JwtSecurityToken(
            issuer: config["Jwt:issuer"],
            audience: config["Jwt:audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );
        
        // Retorno del token JWT generado como una cadena.
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}