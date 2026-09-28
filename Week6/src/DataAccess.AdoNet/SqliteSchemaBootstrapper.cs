using System.Data.Common;

namespace DataAccess.AdoNet;

/// <summary>
/// Creates the same three tables as Sql/CreateTables.sql, in SQLite syntax,
/// so the ADO.NET repositories have somewhere to run against without a real
/// SQL Server — used by both the console/demo app (pointed at a local
/// SQLite file or in-memory DB) and the xUnit tests (always in-memory).
/// Not used in production: a real deployment runs CreateTables.sql +
/// StoredProcedures.sql against SQL Server directly, per Task 4.1.
/// </summary>
public static class SqliteSchemaBootstrapper
{
    public static void EnsureCreated(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Students (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Age INTEGER NOT NULL,
                Email TEXT NOT NULL UNIQUE,
                EnrolledOn TEXT NULL
            );

            CREATE TABLE IF NOT EXISTS Teachers (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Subject TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                Role TEXT NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }
}
