using Microsoft.EntityFrameworkCore;
using La_Divine.Src.Db;

namespace La_Divine.Src.Services;

/// <summary>
/// Servicio en segundo plano que libera automáticamente los asientos reservados
/// cuando una reserva no es pagada dentro de los 10 minutos siguientes a su creación.
/// Se ejecuta cada 60 segundos y cubre el caso de cierre de pestaña o pérdida de sesión.
/// </summary>
public class ReservationCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReservationCleanupService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Espera 60 segundos entre cada verificación
            await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            await CancelExpiredReservationsAsync();
        }
    }

    /// <summary>
    /// Cancela las reservas activas sin pago que superaron los 10 minutos desde su creación
    /// y libera los asientos asociados para que queden disponibles nuevamente.
    /// </summary>
    private async Task CancelExpiredReservationsAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ContextDb>();

            // Límite: reservas creadas hace más de 10 minutos
            var cutoff = DateTime.UtcNow.AddMinutes(-10);

            // Reservas activas (Status = 0), no pagadas y creadas antes del límite
            var expired = await context.Reservations
                .Include(r => r.SelectedSeats)
                .Where(r => !r.IsPaid && r.Status == 0 && r.CreatedAt < cutoff)
                .ToListAsync();

            if (expired.Count == 0) return;

            foreach (var reservation in expired)
            {
                // Marca la reserva como anulada
                reservation.Status = 1;

                // Libera cada asiento para que vuelva a estar disponible
                foreach (var seat in reservation.SelectedSeats)
                    seat.Status = 0;
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"[Limpieza] {expired.Count} reserva(s) expirada(s) anuladas y asientos liberados.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Limpieza] Error al liberar reservas expiradas: {ex.Message}");
        }
    }
}
