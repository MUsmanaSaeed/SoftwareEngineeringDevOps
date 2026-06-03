using SoftwareEngineeringDevOps.App.Users.Repository;

namespace SoftwareEngineeringDevOps.App.Users
{
    public class UsersMediator : IUsersMediator
    {
        IUsersRepository Repository { get; }

        public UsersMediator(IUsersRepository repository)
        {
            Repository = repository;
        }

        public async Task<IEnumerable<IUser>> GetAllUsers()
        {
            await Task.CompletedTask;
            return Repository.ListAll();
        }

        public async Task<IUser?> GetUserById(long id)
        {
            await Task.CompletedTask;
            return Repository.GetById(id);
        }

        public async Task<IUser?> GetUserByUsername(string username)
        {
            await Task.CompletedTask;
            return Repository.GetByUsername(username);
        }

        public async Task<IUser> Insert(NewUser user)
        {
            await Task.CompletedTask;
            return Repository.Insert(user);
        }

        public async Task<IUser> Update(EditUser user)
        {
            await Task.CompletedTask;
            return Repository.Update(user);
        }

        public async Task Delete(long id)
        {
            await Task.CompletedTask;
            Repository.Delete(id);
        }
    }
}
