using System.Data;
using System.Reflection;
using Npgsql;

namespace SoftwareEngineeringDevOps.Database;

public sealed class SqlExecutor
{
    public async Task<IEnumerable<T>> SelectAsync<T>(
        string connectionString,
        string functionName,
        bool isStoredProcedure = true) where T : new()
    {
        return await SelectAsync<T>(connectionString, functionName, null, isStoredProcedure);
    }

    public async Task<IEnumerable<T>> SelectAsync<T>(
        string connectionString,
        string functionName,
        Dictionary<string, object>? parameters,
        bool isStoredProcedure = true) where T : new()
    {
        var results = new List<T>();

        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(functionName, conn)
        {
            CommandType = isStoredProcedure ? CommandType.StoredProcedure : CommandType.Text,
        };

        if (parameters != null)
            foreach (var (key, value) in parameters)
                cmd.Parameters.AddWithValue(key, value);

        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            results.Add(MapToObject<T>(reader));

        return results;
    }

    public async Task ExecuteAsync(
        string connectionString,
        string functionName,
        Dictionary<string, object>? parameters = null,
        bool isStoredProcedure = true)
    {
        await using var conn = new NpgsqlConnection(connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(functionName, conn)
        {
            CommandType = isStoredProcedure ? CommandType.StoredProcedure : CommandType.Text,
        };

        if (parameters != null)
            foreach (var (key, value) in parameters)
                cmd.Parameters.AddWithValue(key, value);

        await cmd.ExecuteNonQueryAsync();
    }

    private static T MapToObject<T>(NpgsqlDataReader reader) where T : new()
    {
        var obj = new T();
        var type = typeof(T);

        for (var i = 0; i < reader.FieldCount; i++)
        {
            if (reader.IsDBNull(i))
                continue;

            var columnName = reader.GetName(i);
            var prop = type.GetProperty(
                columnName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (prop is null)
                continue;

            var value = reader.GetValue(i);
            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            prop.SetValue(obj, Convert.ChangeType(value, targetType));
        }

        return obj;
    }
}
