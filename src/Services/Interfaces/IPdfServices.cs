using La_Divine.Src.Dtos.Reservation;

namespace La_Divine.Src.Services.Interfaces;

/// <summary>
/// Contrato del servicio que genera la entrada en PDF de una reserva (FUN-005).
/// </summary>
public interface IPdfServices
{
    /// <summary>
    /// Genera el PDF de la entrada a partir de los datos de una reserva.
    /// Retorna el archivo en bytes junto con el resultado de la operación.
    /// </summary>
    Task<PdfReservationResponse> GeneratePdfAsync(PdfReservationDto reservationData);
}