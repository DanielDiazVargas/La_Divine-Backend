using La_Divine.Src.Services.Interfaces;

namespace La_Divine.Src.Services.Interfaces;

/// <summary>
/// Interfaz que define los contratos para el envío de correos electrónicos transaccionales mediante SendGrid.
/// </summary>
public interface ISendGridEmailServices
{
    /// <summary>
    /// Envía un correo electrónico utilizando una plantilla dinámica de SendGrid.
    /// </summary>
    Task<bool> SendEmailAsync(string toEmail, string subject, string nombre_funcion, string validacion_string);

    /// <summary>
    /// Envía el correo de anulación de reserva y reembolso (TAR-002), utilizando la plantilla
    /// dinámica de SendGrid con el código de reserva y el monto reembolsado.
    /// </summary>
    Task<bool> SendRefundEmailAsync(string toEmail, string nombre, string codigoReserva, string montoReembolsado);
}