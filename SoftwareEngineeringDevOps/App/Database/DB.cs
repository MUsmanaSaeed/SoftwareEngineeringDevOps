using Microsoft.Extensions.Logging;

namespace SoftwareEngineeringDevOps.App.Database
{
    public class DB
    {
        private static string CONNECTION_STRING => EnvironmentVariables.DB.ToString();
        private readonly SQL_Execute _sqlExecute;
        private readonly ILogger _logger;

        protected DB(SQL_Execute sqlExecute, ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(sqlExecute);
            ArgumentNullException.ThrowIfNull(logger);
            _sqlExecute = sqlExecute;
            _logger = logger;
        }

        protected List<T>? Select<T>(string query, bool isStoredProcedure = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(query);

            try
            {
                List<T>? result = _sqlExecute.Select<T>(CONNECTION_STRING, query, isStoredProcedure);

                if (result == null || result.Count == 0)
                {
                    _logger.LogWarning("SELECT returned no results: {Query}", query);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SELECT: {Query}", query);
                throw;
            }
        }

        protected List<T>? Select<T>(string query, Dictionary<string, object?> parameters, bool isStoredProcedure = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(query);
            ArgumentNullException.ThrowIfNull(parameters);

            try
            {
                List<T>? result = _sqlExecute.Select<T>(CONNECTION_STRING, query, parameters, isStoredProcedure);

                if (result == null || result.Count == 0)
                {
                    _logger.LogWarning("Parameterized SELECT returned no results: {Query}", query);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing parameterized SELECT: {Query}", query);
                throw;
            }
        }

        protected void ExecuteWithParameters(string query, Dictionary<string, object?> parameters, bool isStoredProcedure = true)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(query);
            ArgumentNullException.ThrowIfNull(parameters);

            try
            {
                _sqlExecute.ExecuteWithParameters(CONNECTION_STRING, query, parameters, isStoredProcedure);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing command: {Query}", query);
                throw;
            }
        }
    }
}
