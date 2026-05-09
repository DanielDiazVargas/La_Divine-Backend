using BCrypt.Net;
using La_Divine.Src.Model;

namespace La_Divine.Src.Db;

/// <summary>
/// Clase encargada de insertar datos iniciales
/// en la base de datos.
/// </summary>
public class Seeder(ContextDb context) {

    /// <summary>
    /// Contexto de la base de datos.
    /// </summary>
    private readonly ContextDb _context = context;

    /// <summary>
    /// Inserta roles y usuarios por defecto
    /// si la base de datos se encuentra vacía.
    /// </summary>
    public async Task Seed() {

        // Verifica si existen roles en la base de datos.
        // Si no existen, se crean los roles principales.
        if (!_context.Roles.Any()) {

            var adminRole = new Role { Name = "Admin" };
            var userRole = new Role { Name = "User" };

            await _context.Roles.AddRangeAsync(adminRole, userRole);
            await _context.SaveChangesAsync();
        }
        
    }
}