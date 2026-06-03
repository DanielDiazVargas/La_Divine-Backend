namespace La_Divine.Src.Model;

/// <summary>
/// Entidad que define los niveles de acceso y permisos dentro del sistema (ej. Admin, User).
/// </summary>
public class Role() {
    public int Id { get; set; }
    
    // Nombre descriptivo del rol
    public string Name { get; set; } = string.Empty;
    
    // Colección de usuarios que poseen este rol específico
    public ICollection<User> Users { get; set; } = [];
}