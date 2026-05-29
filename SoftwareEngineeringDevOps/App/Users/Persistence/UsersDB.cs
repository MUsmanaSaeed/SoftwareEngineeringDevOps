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

        public UserDBO Insert(NewUser user)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Username", user.Username },
                { "Password", user.Password },
                { "FirstName", user.FirstName },
                { "LastName", user.LastName ?? string.Empty },
                { "IsAdmin", user.IsAdmin },
                { "IsEditor", user.IsEditor },
            };
            return Select<UserDBO>("users_insert", parameters)?.FirstOrDefault();
        }

        public UserDBO Update(EditUser user)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", user.Id },
                { "Username", user.Username },
                { "Password", user.Password },
                { "FirstName", user.FirstName },
                { "LastName", user.LastName ?? string.Empty },
                { "IsAdmin", user.IsAdmin },
                { "IsEditor", user.IsEditor },
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
