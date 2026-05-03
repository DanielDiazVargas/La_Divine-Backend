using La_Divine.Src.Services.Interfaces;
namespace La_Divine.Src.Services.Interfaces;

public interface ISendGridEmailServices
{
        Task<bool> SendEmailAsync(string toEmail, string subject, string nombre_funcion, string validacion_string);

}