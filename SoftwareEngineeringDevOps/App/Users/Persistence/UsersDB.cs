using SoftwareEngineeringDevOps.App.Database;
using SoftwareEngineeringDevOps.App.Users.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.Users.Persistence
{
    public class UsersDB : DB, IUsersDB
    {
        public IEnumerable<UserDBO> ListAll()
        {
            return Select<UserDBO>("users_listall");
        }

        public UserDBO? GetById(long id)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", id },
            };
            return Select<UserDBO>("users_getbyid", parameters)?.FirstOrDefault();
        }

        public UserDBO? GetByUsername(string username)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Username", username },
            };
            return Select<UserDBO>("users_getbyusername", parameters)?.FirstOrDefault();
        }

        public UserDBO Insert(UserDBO dbo)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Username", dbo.Username },
                { "Password", dbo.Password },
                { "FirstName", dbo.FirstName },
                { "LastName", dbo.LastName },
                { "IsAdmin", dbo.IsAdmin },
                { "IsEditor", dbo.IsEditor },
            };
            return Select<UserDBO>("users_insert", parameters)?.FirstOrDefault();
        }

        public UserDBO Update(UserDBO dbo)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", dbo.Id },
                { "Username", dbo.Username },
                { "Password", dbo.Password },
                { "FirstName", dbo.FirstName },
                { "LastName", dbo.LastName },
                { "IsAdmin", dbo.IsAdmin },
                { "IsEditor", dbo.IsEditor },
            };
            return Select<UserDBO>("users_update", parameters)?.FirstOrDefault();
        }

        public void Delete(long id)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", id },
            };
            ExecuteWithParameters("users_delete", parameters);
        }
    }
}
