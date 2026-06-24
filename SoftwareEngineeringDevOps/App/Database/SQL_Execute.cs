using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Reflection;

namespace SoftwareEngineeringDevOps.App.Database
{
    public class SQL_Execute
    {
        private readonly ILogger<SQL_Execute> _logger;

        public SQL_Execute(ILogger<SQL_Execute> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
        }

        private static object? ConvertDatabaseValueToPropertyType(PropertyInfo property, object value)
        {
            Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            if (propertyType == typeof(DateTime) && value is DateTime dateTimeValue)
            {
                return dateTimeValue.Kind switch
                {
                    DateTimeKind.Utc => dateTimeValue,
                    DateTimeKind.Local => dateTimeValue.ToUniversalTime(),
                    _ => DateTime.SpecifyKind(dateTimeValue, DateTimeKind.Utc)
                };
            }

            if (propertyType == typeof(DateTimeOffset) && value is DateTimeOffset dateTimeOffsetValue)
                return dateTimeOffsetValue.ToUniversalTime();

            return value;
        }

        private static void AssignPropertyValue<T>(T result, PropertyInfo property, object value)
        {
            object? convertedValue = ConvertDatabaseValueToPropertyType(property, value);
            result?.GetType().GetProperty(property.Name)?.SetValue(result, convertedValue);
        }

        private List<T>? Select<T>(NpgsqlCommand command, bool isStoredProcedure, string query)
        {
            try
            {
                PropertyInfo[] properties = typeof(T).GetProperties();

                if (isStoredProcedure)
                    command.CommandType = CommandType.StoredProcedure;
                else
                    command.CommandType = CommandType.Text;

                using NpgsqlDataReader reader = command.ExecuteReader();

                List<T>? results = [];
                int rowCount = 0;

                while (reader.Read())
                {
                    try
                    {
                        T result = Activator.CreateInstance<T>();
                        foreach (PropertyInfo property in properties)
                        {
                            try
                            {
                                if (reader[property.Name] == DBNull.Value)
                                    result?.GetType().GetProperty(property.Name)?.SetValue(result, null);
                                else
                                    AssignPropertyValue(result, property, reader[property.Name]);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Failed to set property {PropertyName} for type {Type}", 
                                    property.Name, typeof(T).Name);
                            }
                        }

                        results.Add(result);
                        rowCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to create instance of type {Type} from database row", typeof(T).Name);
                        throw;
                    }
                }

                return results;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Database error executing SELECT query: {Query}. Error Code: {ErrorCode}", 
                    query, ex.ErrorCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error executing SELECT query: {Query}", query);
                throw;
            }
        }

        public List<T>? Select<T>(string connectionString, string query, bool isStoredProcedure = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            ArgumentException.ThrowIfNullOrWhiteSpace(query);

            try
            {
                using NpgsqlConnection connection = new(connectionString);
                connection.Open();

                using NpgsqlCommand command = new(query, connection);
                return Select<T>(command, isStoredProcedure, query);
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Failed to connect to database or execute query: {Query}", query);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in SELECT operation: {Query}", query);
                throw;
            }
        }

        public List<T>? Select<T>(string connectionString, string query, Dictionary<string, object?> parameters, bool isStoredProcedure = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            ArgumentException.ThrowIfNullOrWhiteSpace(query);
            ArgumentNullException.ThrowIfNull(parameters);

            try
            {
                using NpgsqlConnection connection = new(connectionString);
                connection.Open();

                using NpgsqlCommand command = new(query, connection);
                foreach (KeyValuePair<string, object?> parameter in parameters)
                {
                    var value = parameter.Value ?? DBNull.Value;
                    command.Parameters.AddWithValue(parameter.Key, value);
                }

                return Select<T>(command, isStoredProcedure, query);
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Database error executing parameterized SELECT query: {Query}", query);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in parameterized SELECT operation: {Query}", query);
                throw;
            }
        }

        public void ExecuteWithParameters(string connectionString, string query, Dictionary<string, object?> parameters, bool isStoredProcedure = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
            ArgumentException.ThrowIfNullOrWhiteSpace(query);
            ArgumentNullException.ThrowIfNull(parameters);

            try
            {
#if DEBUG
                NpgsqlConnectionStringBuilder connBuilder = new(connectionString)
                {
                    IncludeErrorDetail = true
                };
                connectionString = connBuilder.ConnectionString;
#endif

                using NpgsqlConnection connection = new(connectionString);
                connection.Open();

                using NpgsqlCommand command = new(query, connection);
                foreach (KeyValuePair<string, object?> parameter in parameters)
                {
                    if (parameter.Value == null)
                    {
                        command.Parameters.AddWithValue(parameter.Key, DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                    }
                }

                if (isStoredProcedure)
                    command.CommandType = CommandType.StoredProcedure;

                int rowsAffected = command.ExecuteNonQuery();
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Database error executing command: {Query}. Error Code: {ErrorCode}", 
                    query, ex.ErrorCode);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error executing command: {Query}", query);
                throw;
            }
        }
    }
}
