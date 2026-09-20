using DataAccess.AdoNet;
using DataAccess.Core.Auth;
using DataAccess.Core.Models;
using Microsoft.Data.Sqlite;
using Xunit;

namespace DataAccess.Tests;

public class AdoNetUserRepositoryTests
{
    private static AdoNetUserRepository CreateRepository(out SqliteConnection keepAlive)
    {
        string connectionString = $"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared";
        keepAlive = new SqliteConnection(connectionString);
        keepAlive.Open();
        SqliteSchemaBootstrapper.EnsureCreated(keepAlive);

        return new AdoNetUserRepository(() => new SqliteConnection(connectionString));
    }

    [Fact]
    public async Task AddAsync_ThenFindByUsernameAsync_RoundTrips()
    {
        var repository = CreateRepository(out var keepAlive);
        using (keepAlive)
        {
            await repository.AddAsync(new User
            {
                Username = "alice.teacher",
                PasswordHash = PasswordHasher.Hash("TeacherPass123!"),
                Role = "Teacher"
            });

            var found = await repository.FindByUsernameAsync("alice.teacher");

            Assert.NotNull(found);
            Assert.Equal("Teacher", found!.Role);
        }
    }

    [Fact]
    public async Task FindByUsernameAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        var repository = CreateRepository(out var keepAlive);
        using (keepAlive)
        {
            var found = await repository.FindByUsernameAsync("nobody");
            Assert.Null(found);
        }
    }

    [Fact]
    public async Task Login_PathWorksEndToEnd_VerifyAgainstStoredHash()
    {
        var repository = CreateRepository(out var keepAlive);
        using (keepAlive)
        {
            await repository.AddAsync(new User
            {
                Username = "bob.student",
                PasswordHash = PasswordHasher.Hash("StudentPass123!"),
                Role = "Student"
            });

            var user = await repository.FindByUsernameAsync("bob.student");

            Assert.True(PasswordHasher.Verify("StudentPass123!", user!.PasswordHash));
            Assert.False(PasswordHasher.Verify("wrong-password", user.PasswordHash));
        }
    }
}
