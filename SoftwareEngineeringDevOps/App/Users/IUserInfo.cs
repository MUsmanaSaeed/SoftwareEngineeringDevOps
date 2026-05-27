namespace SoftwareEngineeringDevOps.App.Users
{
    public interface IUserInfo
    {
        long Id { get; }
        string Username { get; }
        string FirstName { get; }
        string LastName { get; }
        bool IsAdmin { get; }
        bool IsEditor { get; }
    }
}
