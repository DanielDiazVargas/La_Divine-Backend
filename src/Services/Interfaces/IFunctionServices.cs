using La_Divine.Src.Dtos.Function;
using La_Divine.Src.Model;

namespace La_Divine.Src.Services.Interfaces;

public interface IFunctionServices
{
    Task<CreateFunctionResponse> CreateFunctionAsync(CreateFunction request);
}