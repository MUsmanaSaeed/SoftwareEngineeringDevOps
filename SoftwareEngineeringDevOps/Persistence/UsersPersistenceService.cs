using SoftwareEngineeringDevOps.Database;
using SoftwareEngineeringDevOps.Models;
using SoftwareEngineeringDevOps.Persistence.DBO;

namespace SoftwareEngineeringDevOps.Persistence;

public sealed class UsersPersistenceService : DB, IUsersPersistenceService
{
    public UsersPersistenceService(IConfiguration configuration)
        : base(configuration.GetConnectionString("DefaultConnection")
               ?? throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured."))
    {
    }

    public async Task<IEnumerable<User>> ListAllAsync()
    {
        var dbos = await SqlExecutor.SelectAsync<UserDBO>(ConnectionString, "users_listall");
        return dbos.Select(MapToUser);
    }

    public async Task InsertAsync(
        string username,
        string password,
        string firstName,
        string secondName,
        bool isAdmin,
        bool isEditor)
    {
        await SqlExecutor.ExecuteAsync(ConnectionString, "users_insert", new Dictionary<string, object>
        {
            ["Username"] = username,
            ["Password"] = password,
            ["FirstName"] = firstName,
            ["SecondName"] = secondName,
            ["IsAdmin"] = isAdmin,
            ["IsEditor"] = isEditor,
        });
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
        await SqlExecutor.ExecuteAsync(ConnectionString, "users_update", new Dictionary<string, object>
        {
            ["Id"] = id,
            ["Username"] = username,
            ["Password"] = password,
            ["FirstName"] = firstName,
            ["SecondName"] = secondName,
            ["IsAdmin"] = isAdmin,
            ["IsEditor"] = isEditor,
        });
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
        await SqlExecutor.ExecuteAsync(ConnectionString, "users_delete", new Dictionary<string, object>
        {
            ["Id"] = id,
        });
    }

    public async Task EnableAsync(long id)
    {
        await SqlExecutor.ExecuteAsync(ConnectionString, "users_enable", new Dictionary<string, object>
        {
            ["Id"] = id,
        });
    }

    private async Task<UserDBO?> GetDboByUsernameAsync(string username)
    {
        var dbos = await SqlExecutor.SelectAsync<UserDBO>(
            ConnectionString,
            "users_getbyusername",
            new Dictionary<string, object> { ["Username"] = username });

        return dbos.FirstOrDefault();
    }

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
