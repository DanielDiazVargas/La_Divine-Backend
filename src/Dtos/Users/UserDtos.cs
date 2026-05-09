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
}