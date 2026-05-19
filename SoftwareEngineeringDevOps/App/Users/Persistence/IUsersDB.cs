using SoftwareEngineeringDevOps.App.Users.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.Users.Persistence
{
    public interface IUsersDB
    {
        void Delete(long id);
        UserDBO? GetById(long id);
        UserDBO? GetByUsername(string username);
        UserDBO Insert(NewUser user);
        IEnumerable<UserDBO> ListAll();
        UserDBO Update(UserDBO dbo);
    }
}
