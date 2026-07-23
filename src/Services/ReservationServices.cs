using System.Data;
using Microsoft.EntityFrameworkCore;
using La_Divine.Src.Db;
using La_Divine.Src.Dtos.Reservation;
using La_Divine.Src.Dtos.Seat;
using La_Divine.Src.Model;
using La_Divine.Src.Services.Interfaces;

namespace La_Divine.Src.Services;

/// <summary>
/// Servicio que gestiona la lógica de negocio para las reservas del teatro.
/// Incluye creación, consulta y anulación de reservas con control de concurrencia.
/// </summary>
public class ReservationServices(ContextDb contextDb, IPdfServices pdfServices) : IReservationServices
{
    public readonly ContextDb _contextDb = contextDb;
    private readonly IPdfServices _pdfServices = pdfServices;

    /// <summary>
    /// Obtiene todos los asientos de una función con su estado actual.
    /// </summary>
    public async Task<GetSeatsResponse> GetFunctionSeatsAsync(int functionId)
    {
        try
        {
            var function = await _contextDb.Functions.FirstOrDefaultAsync(f => f.Id == functionId);
            if (function == null)
            {
                return new GetSeatsResponse
                {
                    Success = false,
                    Message = "Función no encontrada.",
                    Data = []
                };
            }

            // Lista de asientos asociados a la función solicitada
            var seats = await _contextDb.Seats
                .Where(s => s.FunctionId == functionId)
                .Select(s => new SeatDto
                {
                    Id = s.Id,
                    SeatNumber = s.SeatNumber,
                    Section = s.Section,
                    Price = s.Price,
                    Status = s.Status
                })
                .ToListAsync();

            return new GetSeatsResponse
            {
                Success = true,
                Message = "Asientos obtenidos correctamente.",
                Data = seats
            };
        }
        catch (Exception ex)
        {
            return new GetSeatsResponse
            {
                Success = false,
                Message = $"Error inesperado: {ex.Message}",
                Data = []
            };
        }
    }

    /// <summary>
    /// Crea una reserva para el cliente autenticado con los asientos seleccionados.
    /// Utiliza nivel de aislamiento SERIALIZABLE para evitar reservas simultáneas en el mismo asiento.
    /// </summary>
    public async Task<CreateReservationResponse> CreateReservationAsync(int userId, CreateReservation request)
    {
        if (request.SeatIds == null || request.SeatIds.Count == 0)
        {
            return new CreateReservationResponse
            {
                Success = false,
                Message = "Debe seleccionar al menos un asiento.",
                Data = null!
            };
        }

        if (request.FunctionId <= 0)
        {
            return new CreateReservationResponse
            {
                Success = false,
                Message = "ID de función no válido.",
                Data = null!
            };
        }

        try
        {
            var function = await _contextDb.Functions.FirstOrDefaultAsync(f => f.Id == request.FunctionId);
            if (function == null)
            {
                return new CreateReservationResponse
                {
                    Success = false,
                    Message = "Función no encontrada.",
                    Data = null!
                };
            }

            // Asientos solicitados que pertenecen a la función indicada
            var seatsSelected = await _contextDb.Seats
                .Where(s => request.SeatIds.Contains(s.Id) && s.FunctionId == request.FunctionId)
                .ToListAsync();

            if (seatsSelected.Count != request.SeatIds.Count)
            {
                return new CreateReservationResponse
                {
                    Success = false,
                    Message = "Algunos asientos no fueron encontrados para la función seleccionada.",
                    Data = null!
                };
            }

            // Transacción SERIALIZABLE: impide que dos clientes reserven el mismo asiento al mismo tiempo
            using var transaction = await _contextDb.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                // Asientos que no están en estado Disponible (Status != 0)
                var unavailableSeats = seatsSelected.Where(s => s.Status != 0).ToList();
                if (unavailableSeats.Count != 0)
                {
                    var seatNumbers = string.Join(", ", unavailableSeats.Select(s => s.SeatNumber));
                    await transaction.RollbackAsync();
                    return new CreateReservationResponse
                    {
                        Success = false,
                        Message = $"El asiento seleccionado no se encuentra disponible: {seatNumbers}",
                        Data = null!
                    };
                }

                // Código único de la reserva: UUID sin guiones, primeros 12 caracteres en mayúsculas
                var reservationCode = Guid.NewGuid().ToString("N")[..12].ToUpper();

                // Precio total calculado como la suma de los asientos seleccionados
                var totalPrice = seatsSelected.Sum(s => s.Price);

                var reservation = new Reservation
                {
                    ReservationCode = reservationCode,
                    UserId = userId,
                    FunctionId = request.FunctionId,
                    SelectedSeats = seatsSelected,
                    TotalPrice = totalPrice,
                    Status = 0,
                    CreatedAt = DateTime.UtcNow
                };

                // Marca cada asiento seleccionado como Reservado
                foreach (var seat in seatsSelected)
                {
                    seat.Status = 1;
                }

                await _contextDb.Reservations.AddAsync(reservation);
                await _contextDb.SaveChangesAsync();
                await transaction.CommitAsync();

                // DTO de respuesta con los datos de la reserva creada
                var reservationDto = new ReservationData
                {
                    Id = reservation.Id,
                    ReservationCode = reservation.ReservationCode,
                    FunctionId = reservation.FunctionId,
                    FunctionTitle = function.Name,
                    SelectedSeats = [.. seatsSelected.Select(s => new ReservationSeatDto
                    {
                        Id = s.Id,
                        SeatNumber = s.SeatNumber,
                        Section = s.Section,
                        Price = s.Price
                    })],
                    TotalPrice = reservation.TotalPrice,
                    Status = reservation.Status,
                    CreatedAt = reservation.CreatedAt
                };

                return new CreateReservationResponse
                {
                    Success = true,
                    Message = "Reserva creada exitosamente.",
                    Data = reservationDto
                };
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                return new CreateReservationResponse
                {
                    Success = false,
                    Message = $"Error al guardar la reserva: {ex.Message}",
                    Data = null!
                };
            }
        }
        catch (Exception ex)
        {
            return new CreateReservationResponse
            {
                Success = false,
                Message = $"Error inesperado: {ex.Message}",
                Data = null!
            };
        }
    }

    /// <summary>
    /// Obtiene el historial de reservas del cliente autenticado, ordenadas de la más antigua a la más reciente.
    /// </summary>
    public async Task<MyReservationsResponse> GetMyReservationsAsync(int userId)
    {
        try
        {
            var reservations = await _contextDb.Reservations
                .Include(r => r.Function)
                .Include(r => r.SelectedSeats)
                .Where(r => r.UserId == userId)
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();

            if (reservations.Count == 0)
            {
                return new MyReservationsResponse
                {
                    Success = false,
                    Message = "No hay reservas asociadas a su usuario.",
                    Data = []
                };
            }

            // Transforma cada reserva al DTO de historial
            var data = reservations.Select(r => new MyReservationDto
            {
                ReservationCode = r.ReservationCode,
                FunctionTitle = r.Function.Name,
                FunctionDate = r.Function.DateFunction,
                FunctionTime = r.Function.TimeFunction,
                SeatCount = r.SelectedSeats.Count,
                SeatNumbers = r.SelectedSeats.Select(s => s.SeatNumber).ToList(),
                TotalPrice = r.TotalPrice,
                Status = r.Status,
                IsPaid = r.IsPaid,
                CreatedAt = r.CreatedAt
            }).ToList();

            return new MyReservationsResponse
            {
                Success = true,
                Message = "Reservas obtenidas correctamente.",
                Data = data
            };
        }
        catch (Exception ex)
        {
            return new MyReservationsResponse
            {
                Success = false,
                Message = $"Error inesperado: {ex.Message}",
                Data = []
            };
        }
    }

    /// <summary>
    /// Anula una reserva en estado Pagada y libera los asientos para que estén disponibles nuevamente.
    /// Solo el dueño de la reserva puede anularla.
    /// </summary>
    public async Task<CreateReservationResponse> CancelReservationAsync(string reservationCode, int userId)
    {
        try
        {
            var reservation = await _contextDb.Reservations
                .Include(r => r.SelectedSeats)
                .Include(r => r.Function)
                .FirstOrDefaultAsync(r => r.ReservationCode == reservationCode);

            if (reservation == null)
            {
                return new CreateReservationResponse
                {
                    Success = false,
                    Message = "Reserva no encontrada.",
                    Data = null!
                };
            }

            if (reservation.UserId != userId)
            {
                return new CreateReservationResponse
                {
                    Success = false,
                    Message = "No tiene permiso para anular esta reserva.",
                    Data = null!
                };
            }

            // Solo se pueden anular reservas activas (Status = 0) y no pagadas
            if (reservation.Status != 0 || reservation.IsPaid)
            {
                return new CreateReservationResponse
                {
                    Success = false,
                    Message = "Esta reserva no puede ser anulada.",
                    Data = null!
                };
            }

            using var transaction = await _contextDb.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                // Libera cada asiento asociado dejándolo como Disponible
                foreach (var seat in reservation.SelectedSeats)
                {
                    seat.Status = 0;
                }

                reservation.Status = 1;

                await _contextDb.SaveChangesAsync();
                await transaction.CommitAsync();

                return new CreateReservationResponse
                {
                    Success = true,
                    Message = "Reserva anulada correctamente.",
                    Data = new ReservationData
                    {
                        Id = reservation.Id,
                        ReservationCode = reservation.ReservationCode,
                        FunctionId = reservation.FunctionId,
                        FunctionTitle = reservation.Function.Name,
                        TotalPrice = reservation.TotalPrice,
                        Status = reservation.Status,
                        CreatedAt = reservation.CreatedAt
                    }
                };
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                return new CreateReservationResponse
                {
                    Success = false,
                    Message = $"Error al anular la reserva: {ex.Message}",
                    Data = null!
                };
            }
        }
        catch (Exception ex)
        {
            return new CreateReservationResponse
            {
                Success = false,
                Message = $"Error inesperado: {ex.Message}",
                Data = null!
            };
        }
    }
    // ↑ esta llave cierra CancelReservationAsync

    /// <summary>
    /// Genera la entrada en PDF de una reserva del cliente autenticado.
    /// Solo se permite descargar si la reserva está pagada (FUN-005).
    /// </summary>
    public async Task<PdfReservationResponse> GetReservationPdfAsync(int userId, string reservationCode)
    {
        try
        {
            var reservation = await _contextDb.Reservations
                .Include(r => r.Function)
                .Include(r => r.SelectedSeats)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReservationCode == reservationCode);

            if (reservation == null)
            {
                return new PdfReservationResponse
                {
                    Success = false,
                    Message = "Reserva no encontrada.",
                    Data = []
                };
            }

            if (reservation.UserId != userId)
            {
                return new PdfReservationResponse
                {
                    Success = false,
                    Message = "No tiene permiso para descargar esta entrada.",
                    Data = []
                };
            }

            if (!reservation.IsPaid)
            {
                return new PdfReservationResponse
                {
                    Success = false,
                    Message = "Solo puede descargar la entrada de una reserva pagada.",
                    Data = []
                };
            }

            var pdfDto = new PdfReservationDto
            {
                ReservationCode = reservation.ReservationCode,
                FunctionTitle = reservation.Function.Name,
                FunctionDate = reservation.Function.DateFunction,
                FunctionTime = reservation.Function.TimeFunction,
                HolderName = $"{reservation.User.Name} {reservation.User.LastName}".Trim(),
                SelectedSeats = [.. reservation.SelectedSeats.Select(s => new ReservationSeatDto
                {
                    Id = s.Id,
                    SeatNumber = s.SeatNumber,
                    Section = s.Section,
                    Price = s.Price
                })],
                TotalPrice = reservation.TotalPrice
            };

            return await _pdfServices.GeneratePdfAsync(pdfDto);
        }
        catch (Exception ex)
        {
            return new PdfReservationResponse
            {
                Success = false,
                Message = $"Error al generar el PDF: {ex.Message}",
                Data = []
            };
        }
    }
}
