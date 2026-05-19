namespace SoftwareEngineeringDevOps.App.Database
{
    public class DB
    {
        static string CONNECTION_STRING { get => EnvironmentVariables.DB.ToString(); }

        protected List<T>? Select<T>(string query, bool IsStoredProcedure = true)
        {
            return SQL_Execute.Select<T>(CONNECTION_STRING, query, IsStoredProcedure);
        }
        protected List<T>? Select<T>(string query, Dictionary<string, object?> parameters, bool IsStoredProcedure = true)
        {
            return SQL_Execute.Select<T>(CONNECTION_STRING, query, parameters, IsStoredProcedure);
        }

        protected void ExecuteWithParameters(string query, Dictionary<string, object?> parameters, bool IsStoredProcedure = true)
        {
            SQL_Execute.ExecuteWithParameters(CONNECTION_STRING, query, parameters, IsStoredProcedure);
        }
    }
}
