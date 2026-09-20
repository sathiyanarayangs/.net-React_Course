using DataAccess.Core.Models;
using DataAccess.EfDbFirst;
using Microsoft.EntityFrameworkCore;
using Xunit;
using ScaffoldedDbContext = DataAccess.EfDbFirst.Scaffolded.ScaffoldedDbContext;

namespace DataAccess.Tests;

/// <summary>
/// Task 4.10 - proves EfDbFirstStudentRepository fulfills
/// IRepository&lt;Student&gt; identically to the other two layers, mapping
/// through the scaffolded StudentsRow shape. Uses EF Core InMemory here
/// too (ScaffoldedDbContext takes whatever provider Program.cs configures
/// it with — UseSqlServer in production, UseInMemoryDatabase here).
/// </summary>
public class EfDbFirstRepositoryTests
{
    private static ScaffoldedDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ScaffoldedDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ScaffoldedDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_RoundTrips()
    {
        using var context = CreateContext();
        var repository = new EfDbFirstStudentRepository(context);

        var created = await repository.AddAsync(new Student { Name = "Zoya", Age = 19, Email = "zoya@example.com" });
        var fetched = await repository.GetByIdAsync(created.Id);

        Assert.NotNull(fetched);
        Assert.Equal("Zoya", fetched!.Name);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenMissing()
    {
        using var context = CreateContext();
        var repository = new EfDbFirstStudentRepository(context);

        Assert.False(await repository.UpdateAsync(999, new Student { Name = "X", Age = 1, Email = "x@x.com" }));
    }

    [Fact]
    public async Task DeleteAsync_RemovesStudent()
    {
        using var context = CreateContext();
        var repository = new EfDbFirstStudentRepository(context);
        var created = await repository.AddAsync(new Student { Name = "ToDelete", Age = 20, Email = "del@example.com" });

        Assert.True(await repository.DeleteAsync(created.Id));
        Assert.Null(await repository.GetByIdAsync(created.Id));
    }
}
