using La_Divine.Src.Db;
using La_Divine.Src.Dtos.Function;
using La_Divine.Src.Services.Interfaces;
using La_Divine.Src.Model;
using System;
using Microsoft.Extensions.Configuration; 

namespace La_Divine.Src.Services;

/// <summary>
/// Servicio que gestiona la lógica de negocio para las funciones de teatro.
/// Integra la subida de imágenes, persistencia en base de datos y envío de correos.
/// </summary>
public class FunctionServices(ICloudinaryServices cloudinaryServices, ContextDb contextDb, ISendGridEmailServices sendGridEmailServices, IConfiguration configuration) : IFunctionServices
{
    public readonly ContextDb _contextDb = contextDb;
    public readonly ICloudinaryServices _cloudinaryServices = cloudinaryServices;
    public readonly ISendGridEmailServices _sendGridEmailServices = sendGridEmailServices;
    public readonly IConfiguration _configuration = configuration; 

    /// <summary>
    /// Crea una nueva función, sube el afiche a la nube y notifica al administrador configurado.
    /// </summary>
    public async Task<CreateFunctionResponse> CreateFunctionAsync(CreateFunction request)
    {
        // URL segura que devolverá Cloudinary tras alojar el afiche
        string ResponseImageUrl = string.Empty;
        
        // Identificador para validación interna de la obra
        Guid miUuid = Guid.NewGuid();
        
        try
        {
            if(request.Image != null)
            {
                // Link temporal de la subida a la carpeta
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
            
            // Entidad de la obra de teatro
            var function = new Function
            {
                Name = request.Name,
                Description = request.Description,
                DateFunction = request.DateFunction,
                TimeFunction = request.TimeFunction,
                State = false,
                Image = ResponseImageUrl,
                ValidateFunction = miUuid.ToString("N")
            };
            
            await _contextDb.Functions.AddAsync(function);
            await _contextDb.SaveChangesAsync();

            // Correo del administrador puesto en appsettings
            string adminEmail = _configuration["TeatroConfig:AdminEmail"] ?? "admin@teatroladivine.cl";

            // Estado de éxito o fracaso del envío del correo electrónico
            var response = await _sendGridEmailServices.SendEmailAsync(adminEmail, "Validación de nueva función", function.Name, function.ValidateFunction);
            
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