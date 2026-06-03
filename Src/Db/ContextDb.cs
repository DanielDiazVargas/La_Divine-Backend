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

        modelBuilder.Entity<Role>()
            .HasMany(u=>u.Users)
            .WithOne(r=>r.Role)
            .HasForeignKey(u=>u.RoleId);
    }
}