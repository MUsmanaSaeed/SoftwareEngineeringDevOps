using Npgsql;
using System.Runtime.CompilerServices;

namespace SoftwareEngineeringDevOps
{
    public static class EnvironmentVariables
    {
        private static string GetEnvVar([CallerMemberName] string variable = "") => Environment.GetEnvironmentVariable(variable);


        static string DB_HOST => GetEnvVar();
        static string DB_PORT => GetEnvVar();
        static string DB_NAME => GetEnvVar();
        static string DB_USERID => GetEnvVar();
        static string DB_PASSWORD => GetEnvVar();
        public static NpgsqlConnectionStringBuilder DB => new()
        {
            Host = DB_HOST,
            Port = Convert.ToInt32(DB_PORT),
            Username = DB_USERID,
            Password = DB_PASSWORD,
            Database = DB_NAME,
            PersistSecurityInfo = true,
            IncludeErrorDetail = true,
            SslMode = SslMode.Require,
        };
    }
}
