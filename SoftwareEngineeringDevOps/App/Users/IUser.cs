namespace SoftwareEngineeringDevOps.App.Users
{
    public interface IUser
    {
        long Id { get; }
        string Username { get; }
        string Password { get; }
        string FirstName { get; }
        string LastName { get; }
        bool IsAdmin { get; }
        bool IsEditor { get; }
    }
}
