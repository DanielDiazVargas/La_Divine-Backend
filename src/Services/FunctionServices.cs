using La_Divine.Src.Db;
using La_Divine.Src.Dtos.Function;
using La_Divine.Src.Services.Interfaces;
using La_Divine.Src.Model;
using System;
namespace La_Divine.Src.Services;

public class FunctionServices(ICloudinaryServices cloudinaryServices, ContextDb contextDb, ISendGridEmailServices sendGridEmailServices) : IFunctionServices
{
    public readonly ContextDb _contextDb = contextDb;
    public readonly ICloudinaryServices _cloudinaryServices = cloudinaryServices;
    public readonly ISendGridEmailServices _sendGridEmailServices = sendGridEmailServices;
    public async Task<CreateFunctionResponse> CreateFunctionAsync(CreateFunction request)
    {
        string ResponseImageUrl = string.Empty;
        Guid miUuid = Guid.NewGuid();
        try
        {
            if(request.Image != null)
            {
                var imageUrl = await _cloudinaryServices.UploadImageAsync(request.Image, "functions");
                if(imageUrl == null)
                {
                    return new CreateFunctionResponse
                    {
                        Message = "Error al subir la imagen",
                        Success = false,
                        Data = null!
                    };
                }
                ResponseImageUrl = imageUrl;
            }
            else
            {
                ResponseImageUrl = string.Empty;
            }
            var function = new Function
            {
                Name = request.Name,
                Description = request.Description,
                DateFunction = request.DateFunction,
                TimeFunction = request.TimeFunction,
                State = false,
                Image = ResponseImageUrl,
                // Generar un UUID compacto sin guiones para la validación de la función
                ValidateFunction = miUuid.ToString("N")
            };
            await _contextDb.Functions.AddAsync(function);
            await _contextDb.SaveChangesAsync();
            var response = await _sendGridEmailServices.SendEmailAsync("Francisco.concha.urquietaxd@gmail.com","Prueba",function.Name, function.ValidateFunction);
            if(!response)
            {
                Console.WriteLine("Error al enviar el correo electrónico de validación");
                return new CreateFunctionResponse
                {
                    Message = "Función creada, pero error al enviar el correo electrónico de validación",
                    Success = true,
                    Data = new CreateFunctionData
                    {
                        Id = function.Id,
                        Name = function.Name,
                        Description = function.Description,
                        DateFunction = function.DateFunction,
                        TimeFunction = function.TimeFunction,
                        State = function.State,
                        ImageUrl = function.Image ?? string.Empty
                    }
                };
            }
            return new CreateFunctionResponse
            {
                Message = "Función creada exitosamente",
                Success = true,
                Data = new CreateFunctionData
                {
                    Id = function.Id,
                    Name = function.Name,
                    Description = function.Description,
                    DateFunction = function.DateFunction,
                    TimeFunction = function.TimeFunction,
                    State = function.State,
                    ImageUrl = function.Image ?? string.Empty
                }
            };
        }
        catch (Exception ex)
        {
            return new CreateFunctionResponse
            {
                Message = $"Error al crear la función: {ex.Message}",
                Success = false,
                Data = null!
            };
        }
    }
}
