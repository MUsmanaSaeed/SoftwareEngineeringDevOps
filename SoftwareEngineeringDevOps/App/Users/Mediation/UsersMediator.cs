using SoftwareEngineeringDevOps.App.Users.Persistence;

namespace SoftwareEngineeringDevOps.App.Users
{
    public class UsersMediator : IUsersMediator
    {
        IUsersDB Db { get; }

        public UsersMediator(IUsersDB db) 
        { 
            Db = db;
        }

        public async Task<IEnumerable<IUser>> GetAllUsers()
        {
            await Task.CompletedTask;
            return Db.ListAll().Select(User.FromDBO);
        }

        public async Task<IUser?> GetUserById(long id)
        {
            await Task.CompletedTask;
            var dbo = Db.GetById(id);
            return dbo == null ? null : User.FromDBO(dbo);
        }

        public async Task<IUser?> GetUserByUsername(string username)
        {
            await Task.CompletedTask;
            var dbo = Db.GetByUsername(username);
            return dbo == null ? null : User.FromDBO(dbo);
        }

        public async Task<IUser> Insert(NewUser user)
        {
            await Task.CompletedTask;
            var dbo = Db.Insert(user);
            return User.FromDBO(dbo);
        }

        public async Task<IUser> Update(EditUser user)
        {
            await Task.CompletedTask;
            var dbo = Db.Update(user);
            return User.FromDBO(dbo);
        }

        public async Task Delete(long id)
        {
            await Task.CompletedTask;
            Db.Delete(id);
        }
    }
}
