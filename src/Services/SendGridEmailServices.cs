using La_Divine.Src.Services.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace La_Divine.Src.Services;

public class SendGridEmailServices(IConfiguration configuration) : ISendGridEmailServices
{
    private readonly IConfiguration _configuration = configuration;
    
    /// <summary>
    /// Envía un correo electrónico utilizando SendGrid con una plantilla predefinida. 
    /// </summary>
    public async Task<bool> SendEmailAsync(string toEmail, string subject, string nombre_funcion, string validacion_string)
    {
        try
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var fromEmail = _configuration["SendGrid:FromEmail"];
            var fromName = _configuration["SendGrid:FromName"];
            
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(toEmail);
            
            var msg = new SendGridMessage();
            msg.SetFrom(from);
            msg.AddTo(to);
            
            // Establecer el ID de la plantilla de SendGrid
            msg.SetTemplateId("d-584797d81ae8445ea81b80a00a938585");
            
            // Reemplazamos los datos dinámicos
            msg.SetTemplateData(new
            {
                nombre = nombre_funcion,
                email = toEmail,
                subject = subject,
                validacion_string = validacion_string
            });

            var response = await client.SendEmailAsync(msg);
            
            Console.WriteLine($"Correo electrónico enviado a {toEmail} con estado: {response.StatusCode}. Asunto: {subject}");
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar el correo electrónico: {ex.Message}");
            return false;
        }
    }
}