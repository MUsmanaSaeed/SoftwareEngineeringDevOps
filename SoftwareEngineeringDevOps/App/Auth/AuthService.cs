using SoftwareEngineeringDevOps.App.Users;

namespace SoftwareEngineeringDevOps.App.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUsersMediator _usersMediator;
        private IUser? _currentUser;

        public AuthService(IUsersMediator usersMediator)
        {
            _usersMediator = usersMediator;
        }

        public bool IsAuthenticated => _currentUser != null;
        public IUserInfo? CurrentUser => _currentUser;

        public async Task<IUser?> GetCurrentUser()
        {
            return _currentUser;
        }

        public async Task<bool> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            var user = await _usersMediator.GetUserByUsername(username);
            if (user == null || user.Password != password)
                return false;

            _currentUser = user;
            return true;
        }

        public Task Logout()
        {
            _currentUser = null;
            return Task.CompletedTask;
        }
    }
}
