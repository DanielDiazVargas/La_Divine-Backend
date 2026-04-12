using La_Divine.Src.Dtos.Users;

namespace La_Divine.Src.Services.interfaces;
 
public interface IAuthServices {
    Task<LoginResponse> Login(LoginDto loginDto);
}