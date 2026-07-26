using La_Divine.Src.Db;
using La_Divine.Src.Dtos.Function;
using La_Divine.Src.Services.Interfaces;
using La_Divine.Src.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Globalization;
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

            // Validación ERS: no puede haber dos funciones el mismo día
            var existeEnMismaFecha = await _contextDb.Functions
                .AnyAsync(f => f.DateFunction == request.DateFunction);
            if (existeEnMismaFecha)
            {
                return new CreateFunctionResponse
                {
                    Message = "Ya existe una función registrada para este día",
                    Success = false,
                    Data = null!
                };
            }

            if (request.Image != null)
            {
                // Solo se permiten archivos de imagen como afiche (rechaza pdf, txt, etc.)
                if (!request.Image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    return new CreateFunctionResponse
                    {
                        Message = "El archivo de portada debe ser una imagen (jpg, png, etc.)",
                        Success = false,
                        Data = null!
                    };
                }

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
                State = true,
                Image = responseImageUrl,
                ValidateFunction = functionUuid.ToString("N")
            };

            await _contextDb.Functions.AddAsync(function);
            await _contextDb.SaveChangesAsync();

            // Genera automáticamente los asientos de la función, por sección,
            // con los precios fijos del mapa del teatro (Figura 1).
            var seats = GenerateSeatsForFunction(function.Id);
            await _contextDb.Seats.AddRangeAsync(seats);
            await _contextDb.SaveChangesAsync();

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
        // Obtiene solo funciones activas. Los filtros complejos de fecha y hora
        // se aplican en memoria porque DateFunction y TimeFunction están guardados como texto.
        var functions = await _contextDb.Functions
            .Where(theaterFunction => theaterFunction.State)
            .ToListAsync();

        // Aplica filtro por nombre sin depender de mayúsculas o minúsculas.
        if (!string.IsNullOrWhiteSpace(name))
        {
            functions = functions
                .Where(theaterFunction =>
                    theaterFunction.Name.Contains(name.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        DateOnly? selectedDate = null;

        // Aplica filtro por fecha, pero parseando el texto para evitar errores de formato.
        if (!string.IsNullOrWhiteSpace(date))
        {
            if (!TryParseDateValue(date, out var parsedSelectedDate))
            {
                return new BillboardFunctionResponse
                {
                    Success = false,
                    Message = "La fecha ingresada no tiene un formato válido.",
                    Data = []
                };
            }

            selectedDate = parsedSelectedDate;
        }

        // DateFunction/TimeFunction se ingresan y almacenan como hora local de Chile (texto plano,
        // sin zona horaria). DateTime.Now usa la zona horaria del servidor (normalmente UTC en
        // contenedores), lo que hacía que funciones de hoy con hora futura en Chile se marcaran
        // como "ya pasadas". Se normaliza "now" a hora de Chile para comparar horas equivalentes.
        var chileTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, chileTimeZone);
        var today = DateOnly.FromDateTime(now);

        // Se compara hasta minutos para evitar que una función de las 15:44 desaparezca
        // durante el mismo minuto por los segundos actuales del sistema.
        var currentTime = new TimeOnly(now.Hour, now.Minute);

        // Convierte fecha y hora por separado para evitar que las funciones del día actual
        // sean tratadas como vencidas a las 00:00 horas.
        var validFunctions = functions
            .Select(theaterFunction => new
            {
                TheaterFunction = theaterFunction,
                CanParseDateTime = TryParseFunctionDateTime(
                    theaterFunction,
                    out var functionDate,
                    out var functionTime),
                FunctionDate = functionDate,
                FunctionTime = functionTime
            })
            .Where(item => item.CanParseDateTime)
            .Where(item =>
                selectedDate == null ||
                item.FunctionDate == selectedDate.Value)
            .Where(item =>
                item.FunctionDate > today ||
                item.FunctionDate == today && item.FunctionTime >= currentTime)
            .OrderBy(item => item.FunctionDate)
            .ThenBy(item => item.FunctionTime)
            .ToList();

        var functionIds = validFunctions
            .Select(item => item.TheaterFunction.Id)
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
                BasePrice = group.Min(seat => seat.Price)
            })
            .ToDictionaryAsync(group => group.FunctionId);

        var billboardFunctions = validFunctions
            .Select(item =>
            {
                var theaterFunction = item.TheaterFunction;

                var hasSeatStats = seatStats.TryGetValue(
                    theaterFunction.Id,
                    out var stats);

                return new BillboardFunctionDto
                {
                    Id = theaterFunction.Id,
                    Name = theaterFunction.Name,
                    Description = theaterFunction.Description,
                    DateFunction = theaterFunction.DateFunction,
                    TimeFunction = theaterFunction.TimeFunction,
                    ImageUrl = theaterFunction.Image ?? string.Empty,
                    BasePrice = hasSeatStats ? stats!.BasePrice : 0,
                    AvailableSeats = hasSeatStats ? stats!.AvailableSeats : 0,
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

    /// <summary>
    /// Genera todos los asientos de una función según el mapa del teatro (Figura 1).
    /// Los IDs (SeatNumber) coinciden EXACTAMENTE con los del mapa visual del frontend,
    /// para que la selección y la reserva (FUN-003) funcionen.
    /// </summary>
    private static List<Seat> GenerateSeatsForFunction(int functionId)
    {
        var seats = new List<Seat>();

        // VIP Divine — $85.000 — filas A-E
        AddSection(seats, functionId, "VIP Divine", 85000m, "",
            new[] { ("A", 8), ("B", 9), ("C", 10), ("D", 11), ("E", 12) });

        // Platea — $45.000 — filas F-K
        AddSection(seats, functionId, "Platea", 45000m, "",
            new[] { ("F", 14), ("G", 15), ("H", 16), ("I", 17), ("J", 18), ("K", 19) });

        // Anfiteatro — $28.000 — filas L-Q
        AddSection(seats, functionId, "Anfiteatro", 28000m, "",
            new[] { ("L", 16), ("M", 18), ("N", 20), ("O", 22), ("P", 24), ("Q", 26) });

        // Galería — $18.000 — filas R-T
        AddSection(seats, functionId, "Galería", 18000m, "",
            new[] { ("R", 26), ("S", 28), ("T", 30) });

        // Palco Izquierdo — $65.000 — prefijo "IZQ-"
        AddSection(seats, functionId, "Palco", 65000m, "IZQ-",
            new[] { ("A", 4), ("B", 4), ("C", 5), ("D", 5), ("E", 6), ("F", 6), ("G", 7), ("H", 7) });

        // Palco Derecho — $65.000 — prefijo "DER-"
        AddSection(seats, functionId, "Palco", 65000m, "DER-",
            new[] { ("A", 4), ("B", 4), ("C", 5), ("D", 5), ("E", 6), ("F", 6), ("G", 7), ("H", 7) });

        return seats;
    }

    /// <summary>
    /// Crea los asientos de una sección, fila por fila, con su precio fijo.
    /// </summary>
    private static void AddSection(
        List<Seat> seats, int functionId, string section, decimal price,
        string prefix, (string Row, int Count)[] rows)
    {
        foreach (var (row, count) in rows)
        {
            for (int num = 1; num <= count; num++)
            {
                seats.Add(new Seat
                {
                    SeatNumber = $"{prefix}{row}{num}",
                    Section = section,
                    Price = price,
                    Status = 0, // 0 = Disponible
                    FunctionId = functionId
                });
            }
        }
    }

    public async Task<List<CreateFunctionData>> GetAllFunctionsAsync()
    {
        return await _contextDb.Functions
            .OrderByDescending(f => f.DateFunction)
            .Select(f => new CreateFunctionData
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                DateFunction = f.DateFunction,
                TimeFunction = f.TimeFunction,
                State = f.State,
                ImageUrl = f.Image ?? string.Empty
            })
        .ToListAsync();
    }

    /// <summary>
/// Convierte la fecha y hora de una función en valores comparables.
/// Esto evita que una función del día actual se interprete como vencida a las 00:00.
/// </summary>
private static bool TryParseFunctionDateTime(
    Function theaterFunction,
    out DateOnly functionDate,
    out TimeOnly functionTime)
{
    functionDate = default;
    functionTime = default;

    if (!TryParseDateValue(theaterFunction.DateFunction, out functionDate))
    {
        return false;
    }

    if (!TryParseTimeValue(theaterFunction.TimeFunction, out functionTime))
    {
        return false;
    }

    return true;
}

/// <summary>
/// Convierte una fecha almacenada como texto a DateOnly.
/// Soporta formatos usados por input date y formatos habituales en español.
/// </summary>
private static bool TryParseDateValue(string dateValue, out DateOnly parsedDate)
{
    parsedDate = default;

    if (string.IsNullOrWhiteSpace(dateValue))
    {
        return false;
    }

    var cleanDate = dateValue.Trim();

    // Si por alguna razón la fecha viene como DateTime completo,
    // se conserva solamente la parte de la fecha.
    if (cleanDate.Contains('T'))
    {
        cleanDate = cleanDate.Split('T')[0];
    }
    else if (cleanDate.Contains(' '))
    {
        cleanDate = cleanDate.Split(' ')[0];
    }

    var formats = new[]
    {
        "yyyy-MM-dd",
        "dd-MM-yyyy",
        "dd/MM/yyyy",
        "yyyy/MM/dd"
    };

    foreach (var format in formats)
    {
        if (DateOnly.TryParseExact(
            cleanDate,
            format,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsedDate))
        {
            return true;
        }
    }

    return DateOnly.TryParse(
        cleanDate,
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out parsedDate);
}

/// <summary>
/// Convierte una hora almacenada como texto a TimeOnly.
/// Soporta horas con y sin segundos.
/// </summary>
private static bool TryParseTimeValue(string timeValue, out TimeOnly parsedTime)
{
    parsedTime = default;

    if (string.IsNullOrWhiteSpace(timeValue))
    {
        return false;
    }

    var cleanTime = timeValue.Trim();

    var formats = new[]
    {
        "HH:mm",
        "H:mm",
        "HH:mm:ss",
        "H:mm:ss"
    };

    foreach (var format in formats)
    {
        if (TimeOnly.TryParseExact(
            cleanTime,
            format,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsedTime))
        {
            return true;
        }
    }

    return TimeOnly.TryParse(
        cleanTime,
        CultureInfo.InvariantCulture,
        DateTimeStyles.None,
        out parsedTime);
}
}