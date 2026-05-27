namespace SoftwareEngineeringDevOps.App.Users
{
    public class NewUser
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool IsAdmin { get; set; } = false;
        public bool IsEditor { get; set; } = false;
    }
}
