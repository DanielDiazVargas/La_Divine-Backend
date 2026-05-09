namespace La_Divine.Src.Dtos.Users;

/// <summary>
/// DTO utilizado para recibir los datos
/// del inicio de sesión.
/// </summary>
public class LoginDto {

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña ingresada por el usuario.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Datos retornados cuando el login
/// se realiza correctamente.
/// </summary>
public class DataResponseLogin {

    /// <summary>
    /// Token JWT generado para la sesión.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Id del usuario autenticado.
    /// </summary>
    public int UserId { get; set; }

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
    /// Rol asignado al usuario.
    /// </summary>
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta general del proceso de login.
/// </summary>
public class LoginResponse {

    /// <summary>
    /// Indica si el login fue exitoso.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Mensaje de respuesta del sistema.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Datos del usuario autenticado.
    /// </summary>
    public DataResponseLogin? Data { get; set; }
}