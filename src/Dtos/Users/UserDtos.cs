namespace La_Divine.Src.Dtos.Users
{
    /// <summary>
    /// DTO que recopila los datos ingresados por un cliente nuevo durante su proceso de registro en el sistema.
    /// </summary>
    public class CreateUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rut { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO encargado de transportar de forma temporal las credenciales de un usuario al intentar iniciar sesión.
    /// </summary>
    public class LoginUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO seguro de salida para enviar la información del perfil del usuario al frontend.
    /// Excluye información sensible como la contraseña e incluye información procesada como el nombre del Rol.
    /// </summary>
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rut { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool Status { get; set; }
        public string RoleName { get; set; } = string.Empty; 
    }

    /// <summary>
    /// Objeto de transferencia de datos (DTO) utilizado para procesar la actualización de la contraseña del usuario.
    /// </summary>
    public class ChangePasswordDto
    {
        // TODO:
        // Este UserId es temporal.
        // Cuando el frontend implemente JWT correctamente,
        // el usuario deberá obtenerse desde el token.
        public int UserId { get; set; }

        // Contraseña actual ingresada por el usuario.
        public string ContrasenaActual { get; set; } = string.Empty;

        // Nueva contraseña que desea utilizar.
        public string NuevaContrasena { get; set; } = string.Empty;

        // Confirmación de la nueva contraseña.
        public string ConfirmarContrasena { get; set; } = string.Empty;
    }
}