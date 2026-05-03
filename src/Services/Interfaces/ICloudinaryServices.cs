namespace La_Divine.Src.Services.Interfaces;

using Microsoft.AspNetCore.Http;

public interface ICloudinaryServices
{
    Task<string?> UploadImageAsync(IFormFile file, string folder);
}