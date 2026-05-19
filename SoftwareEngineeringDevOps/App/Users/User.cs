using SoftwareEngineeringDevOps.App.Users.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.Users
{
    public class User : IUser
    {
        public static User FromDBO(UserDBO dbo)
        {
            return new User
            {
                Id = dbo.Id,
                Username = dbo.Username,
                Password = dbo.Password,
                FirstName = dbo.FirstName,
                LastName = dbo.LastName,
                IsAdmin = dbo.IsAdmin,
                IsEditor = dbo.IsEditor,
            };
        }

        public long Id { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public bool IsAdmin { get; private set; }
        public bool IsEditor { get; private set; }
    }
}
