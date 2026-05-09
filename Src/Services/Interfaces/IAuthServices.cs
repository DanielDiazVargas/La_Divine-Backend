using La_Divine.Src.Dtos.Users;

namespace La_Divine.Src.Services.interfaces;

/// <summary>
/// Interfaz encargada de definir los métodos
/// relacionados con la autenticación.
/// </summary>
public interface IAuthServices {

    /// <summary>
    /// Valida las credenciales del usuario
    /// e inicia sesión en el sistema.
    /// </summary>
    /// <param name="loginDto">
    /// Datos ingresados por el usuario para el login.
    /// </param>
    /// <returns>
    /// Retorna una respuesta indicando si
    /// el login fue exitoso o no.
    /// </returns>
    Task<LoginResponse> Login(LoginDto loginDto);
}