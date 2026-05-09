using La_Divine.Src.Dtos.Function;
using La_Divine.Src.Model;

namespace La_Divine.Src.Services.Interfaces;

/// <summary>
/// Interfaz que define los contratos para la gestión de la cartelera de funciones teatrales.
/// </summary>
public interface IFunctionServices
{
    /// <summary>
    /// Crea una nueva función teatral en el sistema, subiendo su afiche y notificando al administrador.
    /// </summary>
    Task<CreateFunctionResponse> CreateFunctionAsync(CreateFunction request);
}