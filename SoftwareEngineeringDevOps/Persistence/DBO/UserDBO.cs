namespace SoftwareEngineeringDevOps.Persistence.DBO;

internal sealed class UserDBO
{
    public long id { get; set; }
    public string username { get; set; } = string.Empty;
    public string password { get; set; } = string.Empty;
    public string firstname { get; set; } = string.Empty;
    public string secondname { get; set; } = string.Empty;
    public bool isadmin { get; set; }
    public bool iseditor { get; set; }
    public bool isdeleted { get; set; }
}
