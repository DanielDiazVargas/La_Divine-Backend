using La_Divine.Src.Services.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace La_Divine.Src.Services;

/// <summary>
/// Servicio encargado de la integración con la API de Cloudinary.
/// Gestiona la subida de archivos multimedia a la nube de forma segura.
/// </summary>
public class CloudinaryServices : ICloudinaryServices
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;

    public CloudinaryServices(IConfiguration configuration)
    {
        // Nombre de la nube configurado en el sistema
        var cloudName = configuration["Cloudinary:CloudName"];
        
        // Clave pública de la API de Cloudinary
        var apiKey = configuration["Cloudinary:ApiKey"];
        
        // Clave secreta privada de la API
        var apiSecret = configuration["Cloudinary:ApiSecret"];
        
        // Objeto de cuenta necesario para autenticar la instancia
        var account = new Account(cloudName, apiKey, apiSecret);
        
        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
    }

    /// <summary>
    /// Sube una imagen física a Cloudinary dentro de una carpeta específica.
    /// Retorna la URL segura donde quedó alojado el archivo.
    /// </summary>
    public async Task<string?> UploadImageAsync(IFormFile file, string folder)
    {
        if(file == null || file.Length == 0)
        {
            return null;
        }
        
        try
        {
            // Flujo de datos crudos de la imagen a subir
            using var stream = file.OpenReadStream();
            
            // Parámetros de configuración de la subida, incluyendo ruta y nombre
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(file.FileName, stream),
                Folder = $"ISAyudantia/divine-teatro/{folder}"
            };
            
            // Respuesta de la API de Cloudinary tras intentar la subida
            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            Console.WriteLine($"Status: {uploadResult.StatusCode}");
            Console.WriteLine($"Error: {uploadResult.Error?.Message}");
            
            if(uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return uploadResult.SecureUrl.ToString();
            }
            return null;

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al subir la imagen: {ex.Message}");
            return null;
        }
    }
}