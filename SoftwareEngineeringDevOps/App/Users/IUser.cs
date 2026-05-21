namespace SoftwareEngineeringDevOps.App.Users
{
    public interface IUser : IUserInfo
    {
        string Password { get; }
    }
}
