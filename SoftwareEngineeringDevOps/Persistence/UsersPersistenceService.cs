using System.Data;
using Npgsql;
using SoftwareEngineeringDevOps.Models;
using SoftwareEngineeringDevOps.Persistence.DBO;

namespace SoftwareEngineeringDevOps.Persistence;

public sealed class UsersPersistenceService : IUsersPersistenceService
{
    private readonly string _connectionString;

    public UsersPersistenceService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured.");
    }

    public async Task<IEnumerable<User>> ListAllAsync()
    {
        var results = new List<User>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("users_listall", conn)
        {
            CommandType = CommandType.StoredProcedure,
        };
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(MapToUser(ReadDbo(reader)));
        }

        return results;
    }

    public async Task InsertAsync(
        string username,
        string password,
        string firstName,
        string secondName,
        bool isAdmin,
        bool isEditor)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("users_insert", conn)
        {
            CommandType = CommandType.StoredProcedure,
        };

        cmd.Parameters.AddWithValue("Username", username);
        cmd.Parameters.AddWithValue("Password", password);
        cmd.Parameters.AddWithValue("FirstName", firstName);
        cmd.Parameters.AddWithValue("SecondName", secondName);
        cmd.Parameters.AddWithValue("IsAdmin", isAdmin);
        cmd.Parameters.AddWithValue("IsEditor", isEditor);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(
        long id,
        string username,
        string password,
        string firstName,
        string secondName,
        bool isAdmin,
        bool isEditor)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("users_update", conn)
        {
            CommandType = CommandType.StoredProcedure,
        };

        cmd.Parameters.AddWithValue("Id", id);
        cmd.Parameters.AddWithValue("Username", username);
        cmd.Parameters.AddWithValue("Password", password);
        cmd.Parameters.AddWithValue("FirstName", firstName);
        cmd.Parameters.AddWithValue("SecondName", secondName);
        cmd.Parameters.AddWithValue("IsAdmin", isAdmin);
        cmd.Parameters.AddWithValue("IsEditor", isEditor);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        var dbo = await GetDboByUsernameAsync(username);
        return dbo is null ? null : MapToUser(dbo);
    }

    public async Task<User?> ValidateCredentialsAsync(string username, string password)
    {
        var dbo = await GetDboByUsernameAsync(username);

        if (dbo is null)
            return null;

        if (dbo.password != password)
            return null;

        return MapToUser(dbo);
    }

    public async Task DeleteAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("users_delete", conn)
        {
            CommandType = CommandType.StoredProcedure,
        };
        cmd.Parameters.AddWithValue("Id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task EnableAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("users_enable", conn)
        {
            CommandType = CommandType.StoredProcedure,
        };
        cmd.Parameters.AddWithValue("Id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    private async Task<UserDBO?> GetDboByUsernameAsync(string username)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("users_getbyusername", conn)
        {
            CommandType = CommandType.StoredProcedure,
        };
        cmd.Parameters.AddWithValue("Username", username);

        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
            return ReadDbo(reader);

        return null;
    }

    private static UserDBO ReadDbo(NpgsqlDataReader reader) => new()
    {
        id = reader.GetInt64(reader.GetOrdinal("id")),
        username = reader.GetString(reader.GetOrdinal("username")),
        password = reader.GetString(reader.GetOrdinal("password")),
        firstname = reader.GetString(reader.GetOrdinal("firstname")),
        secondname = reader.GetString(reader.GetOrdinal("secondname")),
        isadmin = reader.GetBoolean(reader.GetOrdinal("isadmin")),
        iseditor = reader.GetBoolean(reader.GetOrdinal("iseditor")),
        isdeleted = reader.GetBoolean(reader.GetOrdinal("isdeleted")),
    };

    private static User MapToUser(UserDBO dbo) => new()
    {
        Id = dbo.id,
        Username = dbo.username,
        FirstName = dbo.firstname,
        SecondName = dbo.secondname,
        IsAdmin = dbo.isadmin,
        IsEditor = dbo.iseditor,
        IsDeleted = dbo.isdeleted,
    };
}
