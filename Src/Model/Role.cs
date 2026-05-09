namespace La_Divine.Src.Model;

/// <summary>
/// Representa un rol dentro del sistema.
/// </summary>
/// <remarks>
/// Los roles permiten controlar los permisos
/// y accesos de los usuarios.
/// </remarks>
public class Role() {

    /// <summary>
    /// Id único del rol.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre del rol.
    /// </summary>
    /// <example>
    /// Admin, User
    /// </example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Lista de usuarios asociados al rol.
    /// </summary>
    public ICollection<User> Users { get; set; } = [];
}