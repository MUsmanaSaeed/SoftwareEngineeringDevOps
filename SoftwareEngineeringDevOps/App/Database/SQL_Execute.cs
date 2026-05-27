using Npgsql;
using System.Data;
using System.Reflection;

namespace SoftwareEngineeringDevOps.App.Database
{
    public static class SQL_Execute
    {
        static object? ConvertDatabaseValueToPropertyType(PropertyInfo property, object value)
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

        static void AssignPropertyValue<T>(T result, PropertyInfo property, object value)
        {
            object? convertedValue = ConvertDatabaseValueToPropertyType(property, value);
            result?.GetType().GetProperty(property.Name)?.SetValue(result, convertedValue);
        }

        static List<T>? Select<T>(NpgsqlCommand command, bool IsStoredProcedure)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

            if (IsStoredProcedure)
                command.CommandType = CommandType.StoredProcedure;
            else
                command.CommandType = CommandType.Text;
            using NpgsqlDataReader reader = command.ExecuteReader();

            List<T>? results = [];
            while (reader.Read())
            {
                T result = Activator.CreateInstance<T>();
                foreach (PropertyInfo property in properties)
                {
                    if (reader[property.Name] == DBNull.Value)
                        result?.GetType().GetProperty(property.Name)?.SetValue(result, null);
                    else
                        AssignPropertyValue(result, property, reader[property.Name]);
                }

                results.Add(result);
            }
            return results;
        }

        public static List<T>? Select<T>(string connectionString, string query, bool isStoredProcedure = false)
        {
            using NpgsqlConnection connection = new(connectionString);
            connection.Open();

            using NpgsqlCommand command = new(query, connection);
            return Select<T>(command, isStoredProcedure);
        }

        public static List<T>? Select<T>(string connectionString, string query, Dictionary<string, object?> parameters, bool isStoredProcedure = false)
        {
            using NpgsqlConnection connection = new(connectionString);
            connection.Open();

            using NpgsqlCommand command = new(query, connection);
            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                command.Parameters.AddWithValue(parameter.Key, parameter.Value);
            }
            return Select<T>(command, isStoredProcedure);
        }

        public static void ExecuteWithParameters(string connectionString, string query, Dictionary<string, object?> parameters, bool isStoredProcedure = false)
        {
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
                        command.Parameters.AddWithValue(parameter.Key, DBNull.Value);
                    else
                        command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
                if (isStoredProcedure)
                    command.CommandType = CommandType.StoredProcedure;
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
