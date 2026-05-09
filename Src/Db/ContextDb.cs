namespace La_Divine.Src.Model;

using Microsoft.EntityFrameworkCore;

/// <summary>
/// Contexto de la base de datos del sistema.
/// </summary>
/// <remarks>
/// Esta clase se encarga de conectar las entidades
/// del sistema con Entity Framework.
/// </remarks>
public class ContextDb(DbContextOptions<ContextDb> options) : DbContext(options) {

    /// <summary>
    /// Tabla de usuarios.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;

    /// <summary>
    /// Tabla de roles.
    /// </summary>
    public DbSet<Role> Roles { get; set; } = null!;

    /// <summary>
    /// Configuración de relaciones entre entidades.
    /// </summary>
    /// <param name="modelBuilder">
    /// Constructor utilizado por Entity Framework
    /// para configurar el modelo de datos.
    /// </param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Relación entre User y Role
        // Un usuario tiene un rol
        // Un rol puede tener muchos usuarios
        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);

        modelBuilder.Entity<Role>()
            .HasMany(u => u.Users)
            .WithOne(r => r.Role)
            .HasForeignKey(u => u.RoleId);
    }
}