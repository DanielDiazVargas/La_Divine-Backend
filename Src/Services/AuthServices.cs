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
    /// <summary>
    /// Servicio que implementa la lógica de negocio para la autenticación de usuarios.
    /// Maneja el registro de clientes, validación de RUT, encriptación de contraseñas y generación de tokens.
    /// </summary>
    public class AuthServices : IAuthServices
    {
        private readonly ContextDb _context;
        private readonly IConfiguration _config;
        private readonly ISendGridEmailServices _emailServices;

        public AuthServices(ContextDb context, IConfiguration config, ISendGridEmailServices emailServices)
        {
            _context = context;
            _config = config;
            _emailServices = emailServices;
        }

        /// <summary>
        /// Registra un nuevo cliente validando previamente que su RUT y Correo no existan en el sistema.
        /// Genera una contraseña aleatoria y la envía por correo electrónico.
        /// </summary>
        public async Task<UserResponseDto> RegistrarClienteAsync(CreateUserDto createUserDto)
        {
            // Código numérico que indica si el RUT ingresado superó la validación matemática
            int estadoRut = EsRutValido(createUserDto.Rut);

            if (estadoRut == -1)
            {
                throw new Exception("El RUT debe ingresarse sin puntos ni guion (ej.: 12456789K)");
            }
            else if (estadoRut == -2)
            {
                throw new Exception("El RUT Ingresado no es válido.");
            }

            // Convierte el email en minusculas antes de verificar duplicados
            var normalizedEmail = createUserDto.Email.ToLower().Trim();
            // Bandera booleana que verifica si el correo ya está registrado en la base de datos
            var existeCorreo = await _context.Users.AnyAsync(u => u.Email == createUserDto.Email);
            if (existeCorreo)
            {
                throw new Exception("El correo electrónico ingresado ya se encuentra registrado");
            }

            // Bandera booleana que verifica si el RUT ya está asociado a otra cuenta
            var existeRut = await _context.Users.AnyAsync(u => u.Rut == createUserDto.Rut);
            if (existeRut)
            {
                throw new Exception("El RUT ingresado ya se encuentra registrado");
            }

            // Entidad del rol predeterminado para los nuevos registros
            var rolCliente = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
            if (rolCliente == null)
            {
                throw new Exception("El rol de cliente no está configurado en el sistema.");
            }

            // Cadena de texto aleatoria generada como contraseña inicial
            string passwordTemporal = GenerarClaveAlAzar(8);

            // Entidad del nuevo usuario construida con los datos validados y la contraseña hasheada
            var nuevoUsuario = new User
            {
                
                Name = createUserDto.Name,
                LastName = createUserDto.LastName,
                Email = createUserDto.Email.ToLower().Trim(),
                Rut = createUserDto.Rut,
                Phone = createUserDto.Phone,
                Password = BCrypt.Net.BCrypt.HashPassword(passwordTemporal),
                Status = true, 
                RoleId = rolCliente.Id
            };

            _context.Users.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

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

        /// <summary>
        /// Valida las credenciales de ingreso verificando el hash de la contraseña en la base de datos y que el usuario este activo.
        /// </summary>
        public async Task<string> IniciarSesionAsync(LoginUserDto loginUserDto)
        {
            // Convierte el mail en minusculas antes de comparar
            var normalizedEmail = loginUserDto.Email.ToLower().Trim();

            // Buscamos al usuario por correo
            var usuario = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            // Validamos que el usuario exista y la clave sea correcta
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginUserDto.Password, usuario.Password))
            {
                throw new Exception("Correo electrónico o contraseña incorrectos");
            }

            // VALIDACIÓN: Si es usuario activo o inactivo
            if (!usuario.Status)
            {
            throw new Exception("Tu cuenta se encuentra deshabilitada. Contacta al administrador.");
            }

            // Si todo esta correcto se genera el token
            return GenerateToken.CreateToken(usuario, _config);
        }

        // Método encargado de actualizar la contraseña del usuario.
        public async Task<(bool Success, string Message)> ChangePassword(int userId, ChangePasswordDto dto)
        {
            try
            {
      

                // Buscar usuario por Id.
                var user = await _context.Users.FindAsync(userId);

                // Validar existencia del usuario.
                if (user == null)
                    return (false, "El usuario no existe");

                // Validar contraseña actual.
                if (string.IsNullOrWhiteSpace(dto.ContrasenaActual))
                    return (false, "Debe ingresar la contraseña actual");

                // Validar nueva contraseña.
                if (string.IsNullOrWhiteSpace(dto.NuevaContrasena))
                    return (false, "Debe ingresar una nueva contraseña");

                // Validar confirmación.
                if (string.IsNullOrWhiteSpace(dto.ConfirmarContrasena))
                    return (false, "Debe confirmar la nueva contraseña");

                // Eliminar espacios innecesarios.
                dto.NuevaContrasena = dto.NuevaContrasena.Trim();

                // Validar longitud mínima.
                // Esta validación ahora ocurre ANTES de revisar
                // la contraseña actual para facilitar pruebas
                // y mejorar experiencia de usuario.
                if (dto.NuevaContrasena.Length < 8)
                    return (false, "La nueva contraseña debe tener al menos 8 caracteres");

                // Validar longitud máxima.
                // Esto evita inputs extremadamente grandes.
                if (dto.NuevaContrasena.Length > 100)
                    return (false, "La contraseña excede el largo permitido");

                // Verificar coincidencia entre nueva contraseña
                // y confirmación.
                if (dto.NuevaContrasena != dto.ConfirmarContrasena)
                    return (false, "Las contraseñas no coinciden");

                // Verificar contraseña actual usando BCrypt.
                // BCrypt compara el texto ingresado
                // contra el hash almacenado en DB.
                if (!BCrypt.Net.BCrypt.Verify(dto.ContrasenaActual, user.Password))
                    return (false, "La contraseña actual es incorrecta");

                // Generar nuevo hash seguro.
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NuevaContrasena);

                // Guardar cambios en base de datos.
                await _context.SaveChangesAsync();

                // Respuesta exitosa.
                return (true, "Contraseña actualizada correctamente");
            }
                catch (Exception ex)
                {
                    // Captura errores inesperados.
                    return (false, $"Error al cambiar la contraseña: {ex.Message}");
                }
        }   

        /// <summary>
        /// Método privado que implementa el algoritmo del módulo 11 para validar el RUT chileno.
        /// Retorna 0 si es válido, -1 si tiene mal formato y -2 si la estructura matemática falla.
        /// </summary>
        private int EsRutValido(string rut)
        {
            if (string.IsNullOrWhiteSpace(rut) || rut.Contains('.') || rut.Contains('-')) return -1;

            rut = rut.ToUpper();
            if (rut.Length < 2) return -2; 

            // Secuencia de números del RUT excluyendo el dígito verificador
            string cuerpo = rut.Substring(0, rut.Length - 1);
            
            // Dígito final proporcionado en la cadena original
            string digitoIngresado = rut.Substring(rut.Length - 1, 1);

            if (!int.TryParse(cuerpo, out int rutNumerico)) return -2;

            if (rutNumerico < 1000000) return -2;

            // Acumulador para la suma
            int suma = 0;
            
            // Factor multiplicador que va del 2 al 7
            int multiplicador = 2;

            for (int i = cuerpo.Length - 1; i >= 0; i--)
            {
                suma += int.Parse(cuerpo[i].ToString()) * multiplicador;
                multiplicador = multiplicador == 7 ? 2 : multiplicador + 1;
            }

            // Resto de la división por 11
            int resto = suma % 11;
            
            // Valor resultante para determinar el dígito verificador real
            int digitoCalculado = 11 - resto;

            // Dígito verificador matemáticamente correcto
            string digitoEsperado = digitoCalculado switch
            {
                11 => "0",
                10 => "K",
                _ => digitoCalculado.ToString()
            };

            if (digitoIngresado == digitoEsperado)
            {
                return 0; 
            }
            else
            {
                return -2; 
            }
        }

        /// <summary>
        /// Método privado que genera una cadena alfanumérica aleatoria para usarla como contraseña temporal.
        /// </summary>
        private string GenerarClaveAlAzar(int longitud)
        {
            // Conjunto de caracteres permitidos para la generación de la clave
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            
            // Instancia del generador de números pseudoaleatorios
            var random = new Random();
            
            return new string(Enumerable.Repeat(caracteres, longitud)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}