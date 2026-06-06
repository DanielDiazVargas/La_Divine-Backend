namespace La_Divine.Src.Db;

using Microsoft.EntityFrameworkCore;
using La_Divine.Src.Model;
 
/// <summary>
/// Contexto principal de la base de datos utilizando Entity Framework Core.
/// Gestiona la conexión a la base de datos y mapea las clases del modelo a las tablas correspondientes.
/// </summary>
public class ContextDb(DbContextOptions<ContextDb> options) : DbContext(options) {
    
    /// <summary>
    /// Representa la tabla de Usuarios en la base de datos.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;
    
    /// <summary>
    /// Representa la tabla de Roles (Admin, User, etc.) en la base de datos.
    /// </summary>
    public DbSet<Role> Roles { get; set; } = null!;
    
    /// <summary>
    /// Representa la tabla de Funciones teatrales en la base de datos.
    /// </summary>
    public DbSet<Function> Functions { get; set; } = null!;

    /// <summary>
    /// Tabla de asientos del teatro
    /// <summary>
    public DbSet<Seat> Seats { get; set; } = null!;
 
    /// <summary> 
    /// Tabla de reservas realizadas por los clientes
    /// <summary>
    public DbSet<Reservation> Reservations { get; set; } = null!;

    /// <summary>
    /// Configura las relaciones y restricciones de las tablas mediante Fluent API al momento de crear el modelo.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Relación de Uno a Muchos: Un Rol puede tener muchos Usuarios, pero un Usuario tiene solo un Rol.
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);

        // Cada asiento pertenece a una función; si se elimina la función, se eliminan sus asientos
        modelBuilder.Entity<Seat>()
            .HasOne(s => s.Function)
            .WithMany()
            .HasForeignKey(s => s.FunctionId)
            .OnDelete(DeleteBehavior.Cascade);
 
        // Cada reserva pertenece a un cliente
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId);
 
        // Cada reserva está asociada a una función
        modelBuilder.Entity<Reservation>()
            .HasOne(r => r.Function)
            .WithMany()
            .HasForeignKey(r => r.FunctionId);
 
        // Una reserva puede tener muchos asientos y un asiento puede estar en muchas reservas
        modelBuilder.Entity<Reservation>()
            .HasMany(r => r.SelectedSeats)
            .WithMany()
            .UsingEntity(j => j.ToTable("ReservationSeats"));
    }
}