using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using La_Divine.Src.Model;
using Microsoft.IdentityModel.Tokens;

namespace La_Divine.Src.Utils
{
    public static class GenerateToken
    {
        // Función para crear la identidad del usuario
        public static string CreateToken(User user, IConfiguration config)
        {
            // Leer configuración desde appsettings.json
            var jwtSettings = config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Guardar datos básicos en el token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.Name) 
            };

            // Armar el token con fecha de vencimiento
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
                signingCredentials: creds
            );

            // Devolver el token como texto
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}