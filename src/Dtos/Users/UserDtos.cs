namespace La_Divine.Src.Dtos.Users
{
    // DTO para cuando un cliente se registra (USU-002)
    public class CreateUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rut { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    // DTO para cuando un usuario inicia sesión (USU-001)
    public class LoginUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // DTO seguro para devolverle la información al frontend 
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