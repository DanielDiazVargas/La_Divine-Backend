namespace La_Divine.Src.Model;

/// <summary>
/// Entidad principal que representa a un usuario registrado en la plataforma del teatro.
/// Gestiona tanto la información personal como las credenciales de acceso y su nivel de permisos.
/// </summary>
public class User {
    public int Id{get;set;}
    public string Name{get;set;} = string.Empty;
    public string LastName{get;set;} = string.Empty;
    public string Email{get;set;} = string.Empty;
    
    // Documento de identidad (formato sin puntos ni guion)
    public string Rut {get;set;} = string.Empty;
    public string Phone {get;set;} = string.Empty;
    
    // Contraseña encriptada de forma segura utilizando BCrypt
    public string Password {get;set;} = string.Empty;
    
    // Indica si el usuario tiene permitido iniciar sesión en el sistema
    public bool Status {get;set;}
    
    // Entidad de navegación hacia el rol del usuario
    public Role Role { get; set; } = null!;
    
    // Clave foránea que vincula al usuario con un rol específico
    public int RoleId { get; set; }
}