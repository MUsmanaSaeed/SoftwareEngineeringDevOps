namespace SoftwareEngineeringDevOps.App.Users.Repository
{
    public interface IUsersRepository
    {
        IEnumerable<IUser> ListAll();
        IUser? GetById(long id);
        IUser? GetByUsername(string username);
        IUser Insert(NewUser user);
        IUser Update(EditUser user);
        void Delete(long id);
    }
}
