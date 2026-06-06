using La_Divine.Src.Db;
using La_Divine.Src.Dtos.Function;
using La_Divine.Src.Services.Interfaces;
using La_Divine.Src.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace La_Divine.Src.Services;

/// <summary>
/// Servicio que gestiona la lógica de negocio para las funciones de teatro.
/// Integra la subida de imágenes, persistencia en base de datos, envío de correos
/// y consulta de funciones vigentes para cartelera.
/// </summary>
public class FunctionServices(
    ICloudinaryServices cloudinaryServices,
    ContextDb contextDb,
    ISendGridEmailServices sendGridEmailServices,
    IConfiguration configuration
) : IFunctionServices
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
        // URL segura que devolverá Cloudinary tras alojar el afiche.
        string responseImageUrl = string.Empty;

        // Identificador único utilizado para validación interna de la función.
        Guid functionUuid = Guid.NewGuid();

        try
        {
            if (request.Image != null)
            {
                // Sube la imagen del afiche a Cloudinary dentro de la carpeta de funciones.
                var imageUrl = await _cloudinaryServices.UploadImageAsync(request.Image, "functions");

                if (imageUrl == null)
                {
                    return new CreateFunctionResponse
                    {
                        Message = "Error al subir la imagen",
                        Success = false,
                        Data = null!
                    };
                }

                responseImageUrl = imageUrl;
            }

            // Entidad de la función teatral que será almacenada en base de datos.
            var function = new Function
            {
                Name = request.Name,
                Description = request.Description,
                DateFunction = request.DateFunction,
                TimeFunction = request.TimeFunction,
                State = false,
                Image = responseImageUrl,
                ValidateFunction = functionUuid.ToString("N")
            };

            await _contextDb.Functions.AddAsync(function);
            await _contextDb.SaveChangesAsync();

            // Correo del administrador configurado en appsettings.
            string adminEmail = _configuration["TeatroConfig:AdminEmail"] ?? "admin@teatroladivine.cl";

            // Envía correo de validación asociado a la nueva función creada.
            var response = await _sendGridEmailServices.SendEmailAsync(
                adminEmail,
                "Validación de nueva función",
                function.Name,
                function.ValidateFunction
            );

            if (!response)
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

    /// <summary>
    /// Consulta las funciones vigentes disponibles en cartelera.
    /// Aplica filtros opcionales por nombre y fecha, calcula disponibilidad de asientos
    /// y ordena las funciones desde la más próxima hasta la más lejana.
    /// </summary>
    public async Task<BillboardFunctionResponse> GetBillboardFunctionsAsync(string? name, string? date)
    {
        try
        {
            // Consulta base de funciones activas o visibles en cartelera.
            // Se asume que State indica si la función está vigente o habilitada.
            var query = _contextDb.Functions
                .Where(theaterFunction => theaterFunction.State)
                .AsQueryable();

            // Aplica filtro por nombre cuando el usuario ingresa texto.
            if (!string.IsNullOrWhiteSpace(name))
            {
                var normalizedName = name.ToLower();

                query = query.Where(theaterFunction =>
                    theaterFunction.Name.ToLower().Contains(normalizedName));
            }

            // Aplica filtro por fecha cuando el usuario selecciona una fecha.
            if (!string.IsNullOrWhiteSpace(date))
            {
                query = query.Where(theaterFunction =>
                    theaterFunction.DateFunction == date);
            }

            // Se obtienen las funciones antes de ordenar por fecha y hora,
            // ya que actualmente estos campos están almacenados como texto.
            var functions = await query.ToListAsync();

            var functionIds = functions
                .Select(theaterFunction => theaterFunction.Id)
                .ToList();

            // Calcula la cantidad total y disponible de asientos asociados a cada función.
            // Status = 0 corresponde a disponible y Status = 1 corresponde a reservado.
            var seatStats = await _contextDb.Seats
                .Where(seat => functionIds.Contains(seat.FunctionId))
                .GroupBy(seat => seat.FunctionId)
                .Select(group => new
                {
                    FunctionId = group.Key,
                    TotalSeats = group.Count(),
                    AvailableSeats = group.Count(seat => seat.Status == 0),

                    // Se utiliza el menor precio de asiento como precio base temporal.
                    // TODO:
                    // Reemplazar este cálculo si el requerimiento "Registrar Función"
                    // incorpora un campo específico para precio base de la función.
                    BasePrice = group.Min(seat => seat.Price)
                })
                .ToDictionaryAsync(group => group.FunctionId);

            var now = DateTime.Now;

            // Transforma las funciones al DTO de cartelera.
            // También filtra funciones cuya fecha y hora ya pasaron.
            var billboardFunctions = functions
                .Select(theaterFunction =>
                {
                    var canParseDate = DateTime.TryParse(
                        $"{theaterFunction.DateFunction} {theaterFunction.TimeFunction}",
                        out var parsedDate
                    );

                    return new
                    {
                        TheaterFunction = theaterFunction,
                        CanParseDate = canParseDate,
                        ParsedDate = parsedDate
                    };
                })
                .Where(item => item.CanParseDate && item.ParsedDate >= now)
                .OrderBy(item => item.ParsedDate)
                .Select(item =>
                {
                    var theaterFunction = item.TheaterFunction;

                    var hasSeatStats = seatStats.TryGetValue(
                        theaterFunction.Id,
                        out var stats
                    );

                    return new BillboardFunctionDto
                    {
                        Id = theaterFunction.Id,
                        Name = theaterFunction.Name,
                        Description = theaterFunction.Description,
                        DateFunction = theaterFunction.DateFunction,
                        TimeFunction = theaterFunction.TimeFunction,
                        ImageUrl = theaterFunction.Image ?? string.Empty,

                        // Si la función tiene asientos asociados, se usa el precio mínimo real.
                        // Si aún no tiene asientos, se muestra 0 como valor temporal.
                        BasePrice = hasSeatStats ? stats!.BasePrice : 0,

                        // Si aún no se generaron asientos para la función, se muestra 0 disponible.
                        AvailableSeats = hasSeatStats ? stats!.AvailableSeats : 0,

                        // TODO:
                        // Si una función aún no tiene asientos generados, este valor quedará en 0.
                        // Cuando "Registrar Función" cree asientos automáticamente, este dato vendrá desde la tabla Seats.
                        TotalSeats = hasSeatStats ? stats!.TotalSeats : 0
                    };
                })
                .ToList();

            return new BillboardFunctionResponse
            {
                Success = true,
                Message = "Cartelera obtenida correctamente.",
                Data = billboardFunctions
            };
        }
        catch (Exception ex)
        {
            return new BillboardFunctionResponse
            {
                Success = false,
                Message = $"Error al consultar cartelera: {ex.Message}",
                Data = []
            };
        }
    }
}