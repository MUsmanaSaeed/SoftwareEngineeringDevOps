using SoftwareEngineeringDevOps.App.Users;

namespace SoftwareEngineeringDevOps.App.Auth
{
    public interface IAuthService
    {
        Task<IUserInfo?> GetCurrentUserAsync();
        Task<bool> LoginAsync(string username, string password);
        Task LogoutAsync();
        bool IsAuthenticated { get; }
        IUserInfo? CurrentUser { get; }
    }
}
