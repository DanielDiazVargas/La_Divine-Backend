using La_Divine.Src.Model;

namespace La_Divine.Src.Db
{
    public class Seeder
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ContextDb>();

            if (!context.Roles.Any())
            {
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
                var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");

                var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

                if (string.IsNullOrEmpty(adminPassword))
                {
                    if (env == "Production")
                        throw new InvalidOperationException("La variable ADMIN_PASSWORD no está definida.");
                    else
                        adminPassword = "Admin1234";
                }

                var adminUser = new User
                {
                    Name = "Administrador",
                    Email = "admin@ladivine.cl",
                    Password = BCrypt.Net.BCrypt.HashPassword(adminPassword),
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