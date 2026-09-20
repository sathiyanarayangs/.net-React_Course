using System.Data.Common;
using DataAccess.AdoNet;
using DataAccess.Core.Models;
using Microsoft.Data.Sqlite;
using Xunit;

namespace DataAccess.Tests;

/// <summary>
/// Tasks 4.2-4.3 - the ADO.NET repository tested against a real (SQLite,
/// in-memory) database rather than a mock, since the whole point of these
/// tests is proving the ACTUAL SQL executed is safe. A mock could never
/// catch a string-concatenation injection bug; only a real query engine can.
/// Each test opens its own fresh in-memory SQLite connection (":memory:"
/// creates a new, empty database per connection) so tests never interfere.
/// </summary>
public class AdoNetStudentRepositoryTests
{
    private static (AdoNetStudentRepository Repository, DbConnection KeepAliveConnection) CreateRepository()
    {
        // SQLite's :memory: database is destroyed the moment its one
        // connection closes, so a "keep-alive" connection is held open for
        // the test's lifetime while the repository opens its own short-lived
        // connections against the same in-memory database via a shared cache.
        string connectionString = $"Data Source=file:{Guid.NewGuid()}?mode=memory&cache=shared";
        var keepAlive = new SqliteConnection(connectionString);
        keepAlive.Open();
        SqliteSchemaBootstrapper.EnsureCreated(keepAlive);

        var repository = new AdoNetStudentRepository(() => new SqliteConnection(connectionString));
        return (repository, keepAlive);
    }

    [Fact]
    public async Task AddAsync_ThenGetByIdAsync_RoundTrips()
    {
        var (repository, keepAlive) = CreateRepository();
        using (keepAlive)
        {
            var created = await repository.AddAsync(new Student { Name = "Zoya", Age = 19, Email = "zoya@example.com" });
            var fetched = await repository.GetByIdAsync(created.Id);

            Assert.NotNull(fetched);
            Assert.Equal("Zoya", fetched!.Name);
        }
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllAddedStudents()
    {
        var (repository, keepAlive) = CreateRepository();
        using (keepAlive)
        {
            await repository.AddAsync(new Student { Name = "A", Age = 20, Email = "a@example.com" });
            await repository.AddAsync(new Student { Name = "B", Age = 21, Email = "b@example.com" });

            var all = (await repository.GetAllAsync()).ToList();

            Assert.Equal(2, all.Count);
        }
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenStudentMissing()
    {
        var (repository, keepAlive) = CreateRepository();
        using (keepAlive)
        {
            bool updated = await repository.UpdateAsync(999, new Student { Name = "X", Age = 1, Email = "x@x.com" });
            Assert.False(updated);
        }
    }

    [Fact]
    public async Task DeleteAsync_RemovesStudent()
    {
        var (repository, keepAlive) = CreateRepository();
        using (keepAlive)
        {
            var created = await repository.AddAsync(new Student { Name = "ToDelete", Age = 20, Email = "del@example.com" });
            bool deleted = await repository.DeleteAsync(created.Id);

            Assert.True(deleted);
            Assert.Null(await repository.GetByIdAsync(created.Id));
        }
    }

    /// <summary>
    /// Task 4.3's specific proof point: a name containing a SQL injection
    /// payload is stored and retrieved as an ordinary, unexecuted string —
    /// specifically, the Students table still exists afterward, proving
    /// "DROP TABLE" never ran.
    /// </summary>
    [Fact]
    public async Task AddAsync_WithSqlInjectionPayloadAsName_StoresItAsLiteralText()
    {
        var (repository, keepAlive) = CreateRepository();
        using (keepAlive)
        {
            const string injectionPayload = "'; DROP TABLE Students;--";

            var created = await repository.AddAsync(new Student { Name = injectionPayload, Age = 20, Email = "injection@example.com" });
            var fetched = await repository.GetByIdAsync(created.Id);

            Assert.Equal(injectionPayload, fetched!.Name); // stored verbatim, not interpreted as SQL

            // If the injection HAD executed, this second call would throw
            // (no such table) instead of succeeding.
            var allAfterInjectionAttempt = await repository.GetAllAsync();
            Assert.NotEmpty(allAfterInjectionAttempt);
        }
    }
}
