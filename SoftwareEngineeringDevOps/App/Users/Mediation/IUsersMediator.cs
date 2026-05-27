namespace SoftwareEngineeringDevOps.App.Users
{
    public interface IUsersMediator
    {
        Task Delete(long id);
        Task<IEnumerable<IUser>> GetAllUsers();
        Task<IUser?> GetUserById(long id);
        Task<IUser?> GetUserByUsername(string username);
        Task<IUser> Insert(NewUser user);
        Task<IUser> Update(EditUser user);
    }
}
