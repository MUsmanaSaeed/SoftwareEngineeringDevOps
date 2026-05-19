namespace SoftwareEngineeringDevOps.Database;

public abstract class DB
{
    protected string ConnectionString { get; }
    protected SqlExecutor SqlExecutor { get; } = new SqlExecutor();

    protected DB(string connectionString)
    {
        ConnectionString = connectionString;
    }
}
