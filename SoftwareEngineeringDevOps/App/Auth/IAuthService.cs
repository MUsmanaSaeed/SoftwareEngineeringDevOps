using SoftwareEngineeringDevOps.App.Users;

namespace SoftwareEngineeringDevOps.App.Auth
{
    public interface IAuthService
    {
        Task<IUser?> GetCurrentUser();
        Task<bool> Login(string username, string password);
        Task Logout();
        bool IsAuthenticated { get; }
        IUserInfo? CurrentUser { get; }
    }
}
