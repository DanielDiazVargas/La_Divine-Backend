namespace La_Divine.Src.Db;

using Microsoft.EntityFrameworkCore;
using La_Divine.Src.Model;
 
public class ContextDb(DbContextOptions<ContextDb> options) : DbContext(options) {
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Function> Functions { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
