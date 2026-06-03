using La_Divine.Src.Model;

namespace La_Divine.Src.Db
{
    /// <summary>
    /// Clase encargada de poblar (seed) la base de datos con información inicial indispensable.
    /// Genera los roles por defecto y el primer usuario administrador si la base de datos está vacía.
    /// </summary>
    public class Seeder
    {
        /// <summary>
        /// Ejecuta la inyección de datos iniciales de forma asíncrona.
        /// </summary>
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            // Entorno seguro para solicitar servicios de Entity Framework
            using var scope = serviceProvider.CreateScope();
            
            // Instancia del contexto de la base de datos
            var context = scope.ServiceProvider.GetRequiredService<ContextDb>();

            if (!context.Roles.Any())
            {
                // Lista de roles fundamentales del sistema
                var roles = new List<Role>
                {
                    new Role { Name = "Admin" },
                    new Role { Name = "User" }
                };
                await context.Roles.AddRangeAsync(roles);
                await context.SaveChangesAsync();
            }

            if (!context.Users.Any())
            {
                // Referencia al rol de administrador
                var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");

                // Entorno de ejecución actual (Desarrollo o Producción)
                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                
                // Contraseña inyectada desde las variables de entorno
                // Test de seguridad para no hardcodear al administrador
                var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

                if (string.IsNullOrEmpty(adminPassword))
                {
                    if (env == "Production")
                        throw new InvalidOperationException("La variable ADMIN_PASSWORD no está definida.");
                    else
                        adminPassword = "Admin1234";
                }

                // Objeto del primer usuario administrador del sistema
                var adminUser = new User
                {
                    Name = "Administrador",
                    Email = "admin@ladivine.cl",
                    Password = BCrypt.Net.BCrypt.HashPassword(adminPassword),
                    Status = true,
                    RoleId = adminRole!.Id
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();

                if (env != "Production")
                {
                    Console.WriteLine("=== USUARIO ADMINISTRADOR CREADO ===");
                    Console.WriteLine($"Email: {adminUser.Email}");
                    Console.WriteLine($"Contraseña: {adminPassword}");
                    Console.WriteLine("=====================================");
                }
            }
        }
    }
}