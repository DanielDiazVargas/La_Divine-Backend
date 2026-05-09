namespace La_Divine.Src.Services.Interfaces;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Interfaz que define los contratos para la gestión de archivos multimedia en la nube.
/// </summary>
public interface ICloudinaryServices
{
    /// <summary>
    /// Sube un archivo de imagen a Cloudinary dentro de una carpeta específica.
    /// Retorna la URL segura de la imagen subida.
    /// </summary>
    Task<string?> UploadImageAsync(IFormFile file, string folder);
}