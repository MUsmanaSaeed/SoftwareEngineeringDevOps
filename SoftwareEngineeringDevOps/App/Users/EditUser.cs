namespace SoftwareEngineeringDevOps.App.Users
{
    public class EditUser
    {
        public EditUser(IUser user)
        {
            User = user;
            ResetValues();
        }

        IUser User { get; }

        public long Id { get => User.Id; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsEditor { get; set; }

        public void ResetValues()
        {
            Username = User.Username;
            Password = User.Password;
            FirstName = User.FirstName;
            LastName = User.LastName;
            IsAdmin = User.IsAdmin;
            IsEditor = User.IsEditor;
        }
    }
}
