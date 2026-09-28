using System.Data;
using System.Data.Common;
using DataAccess.Core.Models;
using DataAccess.Core.Repositories;

namespace DataAccess.AdoNet;

/// <summary>
/// Tasks 4.2-4.4 - raw ADO.NET (SqlCommand/SqlDataReader shape) implementing
/// the exact same IRepository&lt;Student&gt; the EF Code First and EF DB First
/// layers implement. Written against System.Data.Common (DbConnection,
/// DbCommand) rather than the SqlClient types directly, so the identical
/// code runs against either Microsoft.Data.SqlClient (real SQL Server, via
/// CreateTables.sql/StoredProcedures.sql) or Microsoft.Data.Sqlite
/// (in-memory, for fast automated tests) — only the injected connection
/// factory changes.
///
/// One caveat to that portability claim: identity-column retrieval syntax
/// differs by engine (SQLite's last_insert_rowid() vs SQL Server's
/// SCOPE_IDENTITY()). GetAll/GetById/Update/Delete below are 100% portable
/// standard SQL; AddAsync uses SQLite's syntax since that's the provider
/// the automated tests run against, while AddViaStoredProcedureAsync shows
/// the SQL-Server-correct equivalent through a stored procedure. A
/// production SQL-Server AddAsync would swap SELECT last_insert_rowid()
/// for SELECT CAST(SCOPE_IDENTITY() AS INT).
///
/// EVERY query here uses parameters (@Name, @Id, etc.), never string
/// concatenation/interpolation of user input into SQL text. Task 4.3's
/// injection attempt — naming a student '; DROP TABLE Students;-- — is
/// stored as a literal 32-character string because @Name is bound as a
/// parameter value, never spliced into the SQL statement itself.
/// </summary>
public class AdoNetStudentRepository : IRepository<Student>
{
    private readonly Func<DbConnection> _connectionFactory;

    public AdoNetStudentRepository(Func<DbConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Student>> GetAllAsync()
    {
        using var connection = _connectionFactory();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Age, Email, EnrolledOn FROM Students";

        var students = new List<Student>();
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            students.Add(MapStudent(reader));
        }
        return students;
        // `connection` and `command` dispose here (closing the connection),
        // even if ExecuteReaderAsync or ReadAsync throws — that's what the
        // `using` guarantees regardless of the exception path.
    }

    public async Task<Student?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Name, Age, Email, EnrolledOn FROM Students WHERE Id = @Id";
        AddParameter(command, "@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapStudent(reader) : null;
    }

    public async Task<Student> AddAsync(Student entity)
    {
        using var connection = _connectionFactory();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Students (Name, Age, Email, EnrolledOn)
            VALUES (@Name, @Age, @Email, @EnrolledOn);
            SELECT last_insert_rowid();
            """; // SQLite's identity-retrieval function; see GetByIdViaStoredProcedureAsync
                 // for the SQL-Server-specific SCOPE_IDENTITY() equivalent via a stored proc.
        AddParameter(command, "@Name", entity.Name);
        AddParameter(command, "@Age", entity.Age);
        AddParameter(command, "@Email", entity.Email);
        AddParameter(command, "@EnrolledOn", entity.EnrolledOn ?? (object)DBNull.Value);

        object? newId = await command.ExecuteScalarAsync();
        entity.Id = Convert.ToInt32(newId);
        return entity;
    }

    public async Task<bool> UpdateAsync(int id, Student entity)
    {
        using var connection = _connectionFactory();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Students SET Name = @Name, Age = @Age, Email = @Email, EnrolledOn = @EnrolledOn
            WHERE Id = @Id
            """;
        AddParameter(command, "@Name", entity.Name);
        AddParameter(command, "@Age", entity.Age);
        AddParameter(command, "@Email", entity.Email);
        AddParameter(command, "@EnrolledOn", entity.EnrolledOn ?? (object)DBNull.Value);
        AddParameter(command, "@Id", id);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Students WHERE Id = @Id";
        AddParameter(command, "@Id", id);

        int rowsAffected = await command.ExecuteNonQueryAsync();
        return rowsAffected > 0;
    }

    /// <summary>
    /// Task 4.4 - calls usp_GetStudentById via CommandType.StoredProcedure.
    /// Requires a real SQL Server with StoredProcedures.sql already run —
    /// SQLite has no stored procedures, so this path is exercised manually
    /// (Postman/SSMS) rather than by the SQLite-backed automated tests.
    /// </summary>
    public async Task<Student?> GetByIdViaStoredProcedureAsync(int id)
    {
        using var connection = _connectionFactory();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "usp_GetStudentById";
        command.CommandType = CommandType.StoredProcedure;
        AddParameter(command, "@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapStudent(reader) : null;
    }

    /// <summary>Task 4.4 - calls usp_InsertStudent. Same SQL-Server-only caveat as above.</summary>
    public async Task<Student> AddViaStoredProcedureAsync(Student entity)
    {
        using var connection = _connectionFactory();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "usp_InsertStudent";
        command.CommandType = CommandType.StoredProcedure;
        AddParameter(command, "@Name", entity.Name);
        AddParameter(command, "@Age", entity.Age);
        AddParameter(command, "@Email", entity.Email);
        AddParameter(command, "@EnrolledOn", entity.EnrolledOn ?? (object)DBNull.Value);

        object? newId = await command.ExecuteScalarAsync();
        entity.Id = Convert.ToInt32(newId);
        return entity;
    }

    private static void AddParameter(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }

    private static Student MapStudent(DbDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("Id")),
        Name = reader.GetString(reader.GetOrdinal("Name")),
        Age = reader.GetInt32(reader.GetOrdinal("Age")),
        Email = reader.GetString(reader.GetOrdinal("Email")),
        EnrolledOn = reader.IsDBNull(reader.GetOrdinal("EnrolledOn"))
            ? null
            : reader.GetDateTime(reader.GetOrdinal("EnrolledOn"))
    };
}
