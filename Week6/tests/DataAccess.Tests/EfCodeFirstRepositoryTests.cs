using DataAccess.Core.Models;
using DataAccess.EfCodeFirst;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DataAccess.Tests;

/// <summary>
/// Integration tests for EfStudentRepository using EF Core's InMemory
/// provider — real DbContext, real LINQ translation, no SQL Server needed.
/// Each test gets its own uniquely-named in-memory database so tests never
/// see each other's data.
/// </summary>
public class EfCodeFirstRepositoryTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);
        context.Database.EnsureCreated(); // applies HasData seed rows too
        return context;
    }

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_RoundTrips()
    {
        using var context = CreateContext();
        var repository = new EfStudentRepository(context);

        var created = await repository.AddAsync(new Student { Name = "Zoya", Age = 19, Email = "zoya@example.com" });
        var fetched = await repository.GetByIdAsync(created.Id);

        Assert.NotNull(fetched);
        Assert.Equal("Zoya", fetched!.Name);
    }

    [Fact]
    public async Task GetAllAsync_IncludesSeedData()
    {
        using var context = CreateContext();
        var repository = new EfStudentRepository(context);

        var all = (await repository.GetAllAsync()).ToList();

        Assert.Contains(all, s => s.Name == "Priya Sharma"); // from AppDbContext.OnModelCreating's HasData
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenStudentMissing()
    {
        using var context = CreateContext();
        var repository = new EfStudentRepository(context);

        bool updated = await repository.UpdateAsync(999, new Student { Name = "X", Age = 1, Email = "x@x.com" });

        Assert.False(updated);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesFields_WhenStudentExists()
    {
        using var context = CreateContext();
        var repository = new EfStudentRepository(context);
        var created = await repository.AddAsync(new Student { Name = "Old", Age = 20, Email = "old@example.com" });

        bool updated = await repository.UpdateAsync(created.Id, new Student
        {
            Name = "New",
            Age = 21,
            Email = "new@example.com",
            EnrolledOn = new DateTime(2024, 1, 1)
        });

        var fetched = await repository.GetByIdAsync(created.Id);
        Assert.True(updated);
        Assert.Equal("New", fetched!.Name);
        Assert.Equal(new DateTime(2024, 1, 1), fetched.EnrolledOn);
    }

    [Fact]
    public async Task DeleteAsync_RemovesStudent()
    {
        using var context = CreateContext();
        var repository = new EfStudentRepository(context);
        var created = await repository.AddAsync(new Student { Name = "ToDelete", Age = 20, Email = "del@example.com" });

        bool deleted = await repository.DeleteAsync(created.Id);

        Assert.True(deleted);
        Assert.Null(await repository.GetByIdAsync(created.Id));
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenStudentMissing()
    {
        using var context = CreateContext();
        var repository = new EfStudentRepository(context);

        Assert.False(await repository.DeleteAsync(999));
    }
}
