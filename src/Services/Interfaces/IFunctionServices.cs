using La_Divine.Src.Dtos.Function;

namespace La_Divine.Src.Services.Interfaces;

/// <summary>
/// Interfaz que define las operaciones disponibles para la gestión de funciones teatrales.
/// </summary>
public interface IFunctionServices
{
    /// <summary>
    /// Crea una nueva función teatral en el sistema.
    /// </summary>
    Task<CreateFunctionResponse> CreateFunctionAsync(CreateFunction request);

    /// <summary>
    /// Consulta las funciones vigentes disponibles en cartelera.
    /// Permite aplicar filtros por nombre de obra y fecha.
    /// </summary>
    Task<BillboardFunctionResponse> GetBillboardFunctionsAsync(string? name, string? date);
}