namespace La_Divine.Src.Model;

/// <summary>
/// Representa un usuario del sistema.
/// </summary>
/// <remarks>
/// Puede corresponder a un administrador
/// o un cliente.
/// </remarks>
public class User {

    /// <summary>
    /// Id único del usuario.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// RUT del usuario.
    /// </summary>
    public string Rut { get; set; } = string.Empty;

    /// <summary>
    /// Número telefónico del usuario.
    /// </summary>
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña encriptada del usuario.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Estado del usuario.
    /// </summary>
    /// <remarks>
    /// True = habilitado
    /// False = deshabilitado
    /// </remarks>
    public bool Status { get; set; }

    /// <summary>
    /// Rol asociado al usuario.
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Id del rol asociado.
    /// </summary>
    public int RoleId { get; set; }
}