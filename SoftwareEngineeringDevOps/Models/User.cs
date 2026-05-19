namespace SoftwareEngineeringDevOps.Models;

public sealed class User
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string SecondName { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsEditor { get; set; }
    public bool IsDeleted { get; set; }
}
