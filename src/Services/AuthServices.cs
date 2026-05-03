using La_Divine.Src.Db;
using La_Divine.Src.Dtos.Users;
using La_Divine.Src.Model;
using La_Divine.Src.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; 
using La_Divine.Src.Utils;
using System;
using System.Linq;

namespace La_Divine.Src.Services
{
    public class AuthServices : IAuthServices
    {
        private readonly ContextDb _context;
        private readonly IConfiguration _config;
        
        //  servicio de SendGrid
        private readonly ISendGridEmailServices _emailServices;

        // Inyecta el servicio en el constructor
        public AuthServices(ContextDb context, IConfiguration config, ISendGridEmailServices emailServices)
        {
            _context = context;
            _config = config;
            _emailServices = emailServices;
        }

        public async Task<UserResponseDto> RegistrarClienteAsync(CreateUserDto createUserDto)
        {
            // Guarda el resultado en una variable para revisarlo
            int estadoRut = EsRutValido(createUserDto.Rut);

            if (estadoRut == -1)
            {
                throw new Exception("El RUT debe ingresarse sin puntos ni guion (ej.: 12456789K)");
            }
            else if (estadoRut == -2)
            {
                throw new Exception("El RUT Ingresado no es válido.");
            }

            var existeCorreo = await _context.Users.AnyAsync(u => u.Email == createUserDto.Email);
            if (existeCorreo)
            {
                throw new Exception("El correo electrónico ingresado ya se encuentra registrado");
            }

            var existeRut = await _context.Users.AnyAsync(u => u.Rut == createUserDto.Rut);
            if (existeRut)
            {
                throw new Exception("El RUT ingresado ya se encuentra registrado");
            }

            var rolCliente = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (rolCliente == null)
            {
                throw new Exception("El rol de cliente no está configurado en el sistema.");
            }

            // Genera clave temporal de 8 caracteres al azar
            string passwordTemporal = GenerarClaveAlAzar(8);

            var nuevoUsuario = new User
            {
                Name = createUserDto.Name,
                LastName = createUserDto.LastName,
                Email = createUserDto.Email,
                Rut = createUserDto.Rut,
                Phone = createUserDto.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(passwordTemporal),
                Status = true, 
                RoleId = rolCliente.Id
            };

            _context.Users.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            // Envia el correo con la clave usando SendGrid
            await _emailServices.SendEmailAsync(
                toEmail: nuevoUsuario.Email,
                subject: "Bienvenido a Teatro La Divine - Tu contraseña de acceso",
                nombre_funcion: nuevoUsuario.Name,
                validacion_string: passwordTemporal
            );

            return new UserResponseDto
            {
                Id = nuevoUsuario.Id,
                Name = nuevoUsuario.Name,
                LastName = nuevoUsuario.LastName,
                Email = nuevoUsuario.Email,
                Rut = nuevoUsuario.Rut,
                Phone = nuevoUsuario.Phone,
                Status = nuevoUsuario.Status,
                RoleName = rolCliente.Name
            };
        }

        public async Task<string> IniciarSesionAsync(LoginUserDto loginUserDto)
        {
            var usuario = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == loginUserDto.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginUserDto.Password, usuario.Password))
            {
                throw new Exception("Correo electrónico o contraseña incorrectos");
            }

            return GenerateToken.CreateToken(usuario, _config);
        }

        private int EsRutValido(string rut)
        {
            // Caso Rut con punto o guión
            if (string.IsNullOrWhiteSpace(rut) || rut.Contains('.') || rut.Contains('-')) return -1;

            rut = rut.ToUpper();
            // Rut no válido
            if (rut.Length < 2) return -2; 

            string cuerpo = rut.Substring(0, rut.Length - 1);
            string digitoIngresado = rut.Substring(rut.Length - 1, 1);

            if (!int.TryParse(cuerpo, out int rutNumerico)) return -2;

            int suma = 0;
            int multiplicador = 2;

            for (int i = cuerpo.Length - 1; i >= 0; i--)
            {
                suma += int.Parse(cuerpo[i].ToString()) * multiplicador;
                multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
            }

            int resto = suma % 11;
            int digitoCalculado = 11 - resto;

            string digitoEsperado = digitoCalculado switch
            {
                11 => "0",
                10 => "K",
                _ => digitoCalculado.ToString()
            };

            if (digitoIngresado == digitoEsperado)
            {
                // Exito
                return 0; 
            }
            else
            {
                // Error de estructura matemática (y quita la llave mal puesta)
                return -2; 
            }
        }

        // método para generar la clave aleatoria
        private string GenerarClaveAlAzar(int longitud)
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(caracteres, longitud)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}