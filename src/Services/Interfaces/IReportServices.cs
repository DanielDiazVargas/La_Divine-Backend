using La_Divine.Src.Dtos.Report;

namespace La_Divine.Src.Services.Interfaces;

/// <summary>
/// Interfaz que define las operaciones disponibles para los reportes administrativos.
/// </summary>
public interface IReportServices
{
    /// <summary>
    /// Obtiene el reporte de ventas de las funciones dentro de un rango de fechas.
    /// </summary>
    Task<SalesReportResponse> GetSalesReportAsync(string? startDate, string? endDate);
}