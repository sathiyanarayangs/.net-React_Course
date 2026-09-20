using System.Data.Common;
using DataAccess.Core.Models;
using DataAccess.Core.Repositories;

namespace DataAccess.AdoNet;

/// <summary>
/// Task 4.5 - the Week 3 in-memory login user store, now backed by the
/// Users table via the same parameterized-ADO.NET pattern as
/// AdoNetStudentRepository. The login endpoint itself (verify password,
/// issue token) doesn't change at all — only where the user record comes
/// from changes.
/// </summary>
public class AdoNetUserRepository : IUserStore
{
    private readonly Func<DbConnection> _connectionFactory;

    public AdoNetUserRepository(Func<DbConnection> connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> FindByUsernameAsync(string username)
    {
        using var connection = _connectionFactory();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Username, PasswordHash, Role FROM Users WHERE Username = @Username";
        var parameter = command.CreateParameter();
        parameter.ParameterName = "@Username";
        parameter.Value = username;
        command.Parameters.Add(parameter);

        using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        return new User
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Username = reader.GetString(reader.GetOrdinal("Username")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            Role = reader.GetString(reader.GetOrdinal("Role"))
        };
    }

    public async Task<User> AddAsync(User user)
    {
        using var connection = _connectionFactory();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Users (Username, PasswordHash, Role) VALUES (@Username, @PasswordHash, @Role);
            SELECT last_insert_rowid();
            """;
        AddParam(command, "@Username", user.Username);
        AddParam(command, "@PasswordHash", user.PasswordHash);
        AddParam(command, "@Role", user.Role);

        object? newId = await command.ExecuteScalarAsync();
        user.Id = Convert.ToInt32(newId);
        return user;
    }

    private static void AddParam(DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
