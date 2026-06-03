namespace La_Divine.Src.Dtos.Users;

/// <summary>
/// DTO de salida que contiene la información resumida de un usuario.
/// Se utiliza en el panel de administración para listar los usuarios de forma segura sin exponer datos sensibles.
/// </summary>
public class UserListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rut { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    
    // Estado de la cuenta (true = habilitado, false = deshabilitado)
    public bool Status { get; set; }
    
    // Nombre del rol asignado al usuario (ej. Admin, User)
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// DTO de entrada utilizado para solicitar el cambio de estado de acceso de un usuario en el sistema.
/// </summary>
public class UpdateUserStatusDto
{
    // true para habilitar al usuario, false para bloquear su acceso
    public bool Status { get; set; }
}