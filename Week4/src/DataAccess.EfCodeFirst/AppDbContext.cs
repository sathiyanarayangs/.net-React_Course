using DataAccess.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.EfCodeFirst;

/// <summary>
/// Task 4.6 - EF Core Code First: the C# model here is the source of
/// truth, and Add-Migration/Update-Database (see Migrations/) generate the
/// actual database schema FROM this class, the reverse of DB First.
/// </summary>
public class AppDbContext : DbContext
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<User> Users => Set<User>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fluent API used deliberately here instead of data-annotation
        // attributes on the entity classes, so DataAccess.Core stays free
        // of any EF-specific attributes — the same Student class is also
        // used, unmodified, by the ADO.NET and DB First layers.
        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(s => s.Name).IsRequired().HasMaxLength(100);
            entity.Property(s => s.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(s => s.Email).IsUnique(); // Task 4.6's unique Email index
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.Property(t => t.Name).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Subject).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Username).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(300);
            entity.HasIndex(u => u.Username).IsUnique();
        });

        // Seed data via HasData — applied by the Initial migration.
        modelBuilder.Entity<Student>().HasData(
            new Student { Id = 1, Name = "Priya Sharma", Age = 20, Email = "priya@example.com", EnrolledOn = null }
        );
        modelBuilder.Entity<Teacher>().HasData(
            new Teacher { Id = 1, Name = "Mr. Iyer", Subject = "Physics" }
        );
    }
}
