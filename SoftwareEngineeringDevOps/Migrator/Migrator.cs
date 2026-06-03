using FluentMigrator.Runner;
using Npgsql;
using SoftwareEngineeringDevOps;

namespace Migrator
{
    public class Migrator
    {
        public static void Migrate()
        {
            //host = "192.168.6.170"; user = "postgres"; password = "P0stgre$QLserver"; databaseName = "FactoryTest"; databaseVersion = "1";

            EnsureDatabaseExists();

            IServiceProvider serviceProvider = CreateServices();

            // Put the database update into a scope to ensure
            // that all resources will be disposed.
            using (IServiceScope scope = serviceProvider.CreateScope())
            {
                UpdateDatabase(scope.ServiceProvider);
            }
        }

        private static void EnsureDatabaseExists()
        {
            NpgsqlConnectionStringBuilder connectionString = EnvironmentVariables.DB;
            string? requiredDatabaseName = connectionString.Database;
            //connectionString.Database = null;

            using (NpgsqlConnection con = new(connectionString.ToString()))
            {
                con.Open();
                bool isDatabaseExisting = false;

                using (NpgsqlCommand command = new($"SELECT 1 AS result FROM pg_database WHERE datname = '{requiredDatabaseName}'", con))
                {
                    object? result = command.ExecuteScalar();
                    isDatabaseExisting = result != null;
                }

                if (!isDatabaseExisting)
                {
                    using (NpgsqlCommand command = new($"CREATE DATABASE \"{requiredDatabaseName}\"", con))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Configure the dependency injection services
        /// </sumamry>
        static IServiceProvider CreateServices()
        {
            return new ServiceCollection()
                // Add common FluentMigrator services
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    // Add Postgres support to FluentMigrator
                    .AddPostgres()
                    // Set the connection string
                    .WithGlobalConnectionString(EnvironmentVariables.DB.ToString())
                    // Define the assembly containing the migrations
                    .ScanIn(typeof(V1.Schema).Assembly).For.EmbeddedResources()
                    .ScanIn(typeof(V1.Schema).Assembly).For.Migrations()
                )
                // Enable logging to console in the FluentMigrator way
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .Configure<FluentMigratorLoggerOptions>((cfg) =>
                {
                    cfg.ShowSql = false;
                    cfg.ShowElapsedTime = true;
                })
                // Build the service provider
                .BuildServiceProvider(false);
        }

        /// <summary>
        /// Update the database
        /// </summary>
        static void UpdateDatabase(IServiceProvider serviceProvider)
        {
            // Instantiate the runner
            IMigrationRunner runner = serviceProvider.GetRequiredService<IMigrationRunner>();

            if (runner.HasMigrationsToApplyUp())
                runner.MigrateUp();
        }
    }
}
