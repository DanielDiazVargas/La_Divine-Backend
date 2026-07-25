using System.Globalization;
using La_Divine.Src.Db;
using La_Divine.Src.Dtos.Report;
using La_Divine.Src.Model;
using La_Divine.Src.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace La_Divine.Src.Services;

/// <summary>
/// Servicio que construye reportes administrativos a partir de funciones, reservas y asientos.
/// </summary>
public class ReportServices : IReportServices
{
    private readonly ContextDb _contextDb;

    public ReportServices(ContextDb contextDb)
    {
        _contextDb = contextDb;
    }

    /// <summary>
    /// Obtiene el reporte de ventas de las funciones cuya fecha está dentro del rango indicado.
    /// El rango se aplica sobre la fecha de la función y no sobre la fecha de pago.
    /// </summary>
    public async Task<SalesReportResponse> GetSalesReportAsync(string? startDate, string? endDate)
    {
        if (string.IsNullOrWhiteSpace(startDate))
        {
            return BuildErrorResponse("Debe ingresar una fecha de inicio.");
        }

        if (string.IsNullOrWhiteSpace(endDate))
        {
            return BuildErrorResponse("Debe ingresar una fecha de término.");
        }

        if (!TryParseDate(startDate, out var parsedStartDate))
        {
            return BuildErrorResponse("La fecha de inicio no tiene un formato válido.");
        }

        if (!TryParseDate(endDate, out var parsedEndDate))
        {
            return BuildErrorResponse("La fecha de término no tiene un formato válido.");
        }

        if (parsedStartDate > parsedEndDate)
        {
            return BuildErrorResponse("La fecha de inicio no puede ser posterior a la fecha de término");
        }

        try
        {
            // Actualmente DateFunction está almacenada como texto, por eso se filtra en memoria
            // luego de convertir la fecha a DateOnly.
            var functions = await _contextDb.Functions
                .AsNoTracking()
                .ToListAsync();

            var functionsInRange = functions
                .Select(theaterFunction => new
                {
                    TheaterFunction = theaterFunction,
                    CanParseDate = TryParseDate(theaterFunction.DateFunction, out var parsedFunctionDate),
                    ParsedFunctionDate = parsedFunctionDate
                })
                .Where(item =>
                    item.CanParseDate &&
                    item.ParsedFunctionDate >= parsedStartDate &&
                    item.ParsedFunctionDate <= parsedEndDate)
                .OrderBy(item => item.ParsedFunctionDate)
                .ThenBy(item => item.TheaterFunction.TimeFunction)
                .Select(item => item.TheaterFunction)
                .ToList();

            if (functionsInRange.Count == 0)
            {
                return BuildErrorResponse("No existen registros para el período indicado");
            }

            var functionIds = functionsInRange
                .Select(theaterFunction => theaterFunction.Id)
                .ToList();

            // Obtiene todos los asientos de las funciones del período para calcular ocupación.
            var seats = await _contextDb.Seats
                .AsNoTracking()
                .Where(seat => functionIds.Contains(seat.FunctionId))
                .ToListAsync();

            // Se consideran solo reservas activas. Las anuladas, reembolsadas o en proceso
            // de anulación no se incorporan al reporte de ventas.
            var reservations = await _contextDb.Reservations
                .AsNoTracking()
                .Include(reservation => reservation.Function)
                .Include(reservation => reservation.SelectedSeats)
                .Where(reservation =>
                    functionIds.Contains(reservation.FunctionId) &&
                    reservation.Status == 0)
                .ToListAsync();

            var report = new SalesReportDto
            {
                StartDate = parsedStartDate.ToString("yyyy-MM-dd"),
                EndDate = parsedEndDate.ToString("yyyy-MM-dd"),
                Summary = BuildSummary(functionsInRange, reservations),
                FunctionSales = BuildFunctionSales(functionsInRange, reservations),
                TicketDetails = BuildTicketDetails(reservations),
                OccupancyByFunction = BuildOccupancy(functionsInRange, seats, reservations)
            };

            // Si existen funciones en el rango, pero no hay ventas ni reservas,
            // se responde con el mensaje indicado por el requerimiento.
            var hasReportData = reservations.Count > 0;

            if (!hasReportData)
            {
                return BuildErrorResponse("No existen registros para el período indicado");
            }

            return new SalesReportResponse
            {
                Success = true,
                Message = "Reporte de ventas obtenido correctamente.",
                Data = report
            };
        }
        catch (Exception ex)
        {
            return BuildErrorResponse($"Error al generar reporte de ventas: {ex.Message}");
        }
    }

    /// <summary>
    /// Construye el resumen general con entradas pagadas, entradas pendientes e ingresos confirmados.
    /// </summary>
    private static SalesReportSummaryDto BuildSummary(
        List<Function> functions,
        List<Reservation> reservations)
    {
        var paidReservations = reservations
            .Where(reservation => reservation.IsPaid)
            .ToList();

        var pendingReservations = reservations
            .Where(reservation => !reservation.IsPaid)
            .ToList();

        return new SalesReportSummaryDto
        {
            FunctionCount = functions.Count,
            TotalSoldTickets = paidReservations.Sum(reservation => reservation.SelectedSeats.Count),
            TotalPendingTickets = pendingReservations.Sum(reservation => reservation.SelectedSeats.Count),
            TotalRevenue = paidReservations.Sum(reservation => reservation.TotalPrice)
        };
    }

    /// <summary>
    /// Construye la tabla de ingresos por función, ordenada desde el mayor ingreso al menor.
    /// </summary>
    private static List<FunctionSalesReportDto> BuildFunctionSales(
        List<Function> functions,
        List<Reservation> reservations)
    {
        return functions
            .Select(theaterFunction =>
            {
                var functionReservations = reservations
                    .Where(reservation => reservation.FunctionId == theaterFunction.Id)
                    .ToList();

                var paidReservations = functionReservations
                    .Where(reservation => reservation.IsPaid)
                    .ToList();

                var pendingReservations = functionReservations
                    .Where(reservation => !reservation.IsPaid)
                    .ToList();

                return new FunctionSalesReportDto
                {
                    FunctionId = theaterFunction.Id,
                    FunctionName = theaterFunction.Name,
                    DateFunction = theaterFunction.DateFunction,
                    TimeFunction = theaterFunction.TimeFunction,
                    SoldTickets = paidReservations.Sum(reservation => reservation.SelectedSeats.Count),
                    PendingTickets = pendingReservations.Sum(reservation => reservation.SelectedSeats.Count),
                    TotalRevenue = paidReservations.Sum(reservation => reservation.TotalPrice)
                };
            })
            .OrderByDescending(functionReport => functionReport.TotalRevenue)
            .ThenBy(functionReport => functionReport.DateFunction)
            .ThenBy(functionReport => functionReport.TimeFunction)
            .ToList();
    }

    /// <summary>
    /// Construye el detalle de entradas individuales. Cada asiento reservado se muestra como una entrada.
    /// </summary>
    private static List<TicketSalesReportDto> BuildTicketDetails(List<Reservation> reservations)
    {
        return reservations
            .OrderBy(reservation => reservation.Function.DateFunction)
            .ThenBy(reservation => reservation.Function.TimeFunction)
            .ThenBy(reservation => reservation.ReservationCode)
            .SelectMany(reservation => reservation.SelectedSeats
                .OrderBy(seat => seat.SeatNumber)
                .Select(seat => new TicketSalesReportDto
                {
                    ReservationCode = reservation.ReservationCode,
                    FunctionId = reservation.FunctionId,
                    FunctionName = reservation.Function.Name,
                    DateFunction = reservation.Function.DateFunction,
                    TimeFunction = reservation.Function.TimeFunction,
                    SeatNumber = seat.SeatNumber,
                    Section = seat.Section,
                    SeatPrice = seat.Price,

                    // Solo las entradas pagadas suman al monto ganado del reporte.
                    RevenueAmount = reservation.IsPaid ? seat.Price : 0,
                    PaymentStatus = reservation.IsPaid ? "Pagada" : "Pendiente"
                }))
            .ToList();
    }

    /// <summary>
    /// Construye los porcentajes de ocupación general y por sección para cada función.
    /// </summary>
    private static List<FunctionOccupancyReportDto> BuildOccupancy(
        List<Function> functions,
        List<Seat> seats,
        List<Reservation> reservations)
    {
        return functions
            .Select(theaterFunction =>
            {
                var functionSeats = seats
                    .Where(seat => seat.FunctionId == theaterFunction.Id)
                    .ToList();

                var paidSeatIds = reservations
                    .Where(reservation =>
                        reservation.FunctionId == theaterFunction.Id &&
                        reservation.IsPaid)
                    .SelectMany(reservation => reservation.SelectedSeats.Select(seat => seat.Id))
                    .ToHashSet();

                var pendingSeatIds = reservations
                    .Where(reservation =>
                        reservation.FunctionId == theaterFunction.Id &&
                        !reservation.IsPaid)
                    .SelectMany(reservation => reservation.SelectedSeats.Select(seat => seat.Id))
                    .ToHashSet();

                var soldSeats = functionSeats.Count(seat => paidSeatIds.Contains(seat.Id));
                var pendingSeats = functionSeats.Count(seat => pendingSeatIds.Contains(seat.Id));
                var totalSeats = functionSeats.Count;

                return new FunctionOccupancyReportDto
                {
                    FunctionId = theaterFunction.Id,
                    FunctionName = theaterFunction.Name,
                    DateFunction = theaterFunction.DateFunction,
                    TimeFunction = theaterFunction.TimeFunction,

                    // El modelo actual no posee entidad Sala. Se usa un nombre fijo
                    // hasta que otro requerimiento incorpore una tabla de salas.
                    RoomName = "Sala principal",
                    SoldSeats = soldSeats,
                    PendingSeats = pendingSeats,
                    TotalSeats = totalSeats,
                    OccupancyPercentage = CalculatePercentage(soldSeats, totalSeats),
                    Sections = BuildSectionOccupancy(functionSeats, paidSeatIds, pendingSeatIds)
                };
            })
            .OrderBy(occupancy => occupancy.DateFunction)
            .ThenBy(occupancy => occupancy.TimeFunction)
            .ToList();
    }

    /// <summary>
    /// Construye los porcentajes de ocupación por sección, incluyendo secciones con 0% de ocupación.
    /// </summary>
    private static List<SectionOccupancyReportDto> BuildSectionOccupancy(
        List<Seat> functionSeats,
        HashSet<int> paidSeatIds,
        HashSet<int> pendingSeatIds)
    {
        return functionSeats
            .GroupBy(seat => seat.Section)
            .OrderBy(group => GetSectionOrder(group.Key))
            .ThenBy(group => group.Key)
            .Select(group =>
            {
                var totalSeats = group.Count();
                var soldSeats = group.Count(seat => paidSeatIds.Contains(seat.Id));
                var pendingSeats = group.Count(seat => pendingSeatIds.Contains(seat.Id));

                return new SectionOccupancyReportDto
                {
                    Section = group.Key,
                    SoldSeats = soldSeats,
                    PendingSeats = pendingSeats,
                    TotalSeats = totalSeats,
                    OccupancyPercentage = CalculatePercentage(soldSeats, totalSeats)
                };
            })
            .ToList();
    }

    /// <summary>
    /// Calcula un porcentaje redondeado a dos decimales, evitando división por cero.
    /// </summary>
    private static decimal CalculatePercentage(int value, int total)
    {
        if (total == 0)
        {
            return 0;
        }

        return Math.Round(value * 100m / total, 2);
    }

/// <summary>
/// Convierte una fecha de texto a DateOnly usando formatos habituales del proyecto.
/// También limpia valores que vengan como DateTime completo.
/// </summary>
private static bool TryParseDate(string dateValue, out DateOnly parsedDate)
{
    parsedDate = default;

    if (string.IsNullOrWhiteSpace(dateValue))
    {
        return false;
    }

    var cleanDate = dateValue.Trim();

    // Si la fecha viene como DateTime completo, se conserva solo la parte de fecha.
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
    /// Define el orden visual de las secciones de acuerdo con el mapa del teatro.
    /// </summary>
    private static int GetSectionOrder(string section)
    {
        return section switch
        {
            "VIP Divine" => 1,
            "Platea" => 2,
            "Anfiteatro" => 3,
            "Galería" => 4,
            "Palco" => 5,
            _ => 99
        };
    }

    /// <summary>
    /// Construye una respuesta de error manteniendo el mismo formato del endpoint.
    /// </summary>
    private static SalesReportResponse BuildErrorResponse(string message)
    {
        return new SalesReportResponse
        {
            Success = false,
            Message = message,
            Data = null
        };
    }
}