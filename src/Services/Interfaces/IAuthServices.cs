using La_Divine.Src.Dtos.Users;

namespace La_Divine.Src.Services.Interfaces
{
    /// <summary>
    /// Interfaz que define los contratos para el servicio de autenticación.
    /// </summary>
    public interface IAuthServices
    {
        /// <summary>
        /// Registra un nuevo cliente en el sistema y le asigna una contraseña temporal.
        /// </summary>
        Task<UserResponseDto> RegistrarClienteAsync(CreateUserDto createUserDto);

        /// <summary>
        /// Valida las credenciales de un usuario y genera un token JWT para su sesión.
        /// </summary>
        Task<string> IniciarSesionAsync(LoginUserDto loginUserDto);
    }
}