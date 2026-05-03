using La_Divine.Src.Dtos.Users;

namespace La_Divine.Src.Services.Interfaces
{
    public interface IAuthServices
    {
        
        // Función para registrar
        Task<UserResponseDto> RegistrarClienteAsync(CreateUserDto createUserDto);

        // Función para iniciar sesión
        Task<string> IniciarSesionAsync(LoginUserDto loginUserDto);
    }
}