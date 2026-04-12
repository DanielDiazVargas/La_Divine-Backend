using BCrypt.Net;
using La_Divine.Src.Model;

namespace La_Divine.Src.Db;

public class Seeder(ContextDb context) {
    private readonly ContextDb _context = context;

    public async Task Seed() {

        // Verificación de la existencia de roles en la base de datos. 
        // Si no existen, se crean los roles de administrador y usuario.
        if (!_context.Roles.Any()) {
            var adminRole = new Role { Name = "Admin" };
            var userRole = new Role { Name = "User" };

            await _context.Roles.AddRangeAsync(adminRole, userRole);
            await _context.SaveChangesAsync();
        }

        // Verificación de la existencia de usuarios en la base de datos.
        // Si no existen, se crean un usuario administrador y un usuario normal 
        // con contraseñas hasheadas utilizando BCrypt.
        if (!_context.Users.Any()) {
            var adminUser = new User {
                Name = "Admin",
                LastName = "User",
                Email = "admin@divine.cl",
                Rut = "12345678-9",
                Phone = "123456789",
                Password = BCrypt.Net.BCrypt.HashPassword("admin123"), 
                Status = true,
                RoleId = _context.Roles.First(r => r.Name == "Admin").Id
            };  
            var normalUser = new User {
                Name = "Normal",
                LastName = "User",
                Email = "user@divine.cl",
                Rut = "98765432-1",
                Phone = "987654321",
                Password = BCrypt.Net.BCrypt.HashPassword("user123"), // Hasheamos la contraseña para el usuario normal
                Status = true,
                RoleId = _context.Roles.First(r => r.Name == "User").Id
            };

            await _context.Users.AddRangeAsync(adminUser, normalUser);
            await _context.SaveChangesAsync();
        }
    }
}