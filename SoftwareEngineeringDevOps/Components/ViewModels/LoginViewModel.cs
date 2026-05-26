using SoftwareEngineeringDevOps.App.Auth;

namespace SoftwareEngineeringDevOps.Components.ViewModels
{
    public class LoginViewModel
    {
        private readonly IAuthService _authService;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public bool IsLoading { get; set; }
        public string? ReturnUrl { get; set; }

        public async Task<bool> ExecuteLogin()
        {
            ErrorMessage = null;
            IsLoading = true;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Username and password are required.";
                IsLoading = false;
                return false;
            }

            var result = await _authService.Login(Username, Password);
            if (!result)
            {
                ErrorMessage = "Invalid username or password.";
                IsLoading = false;
                return false;
            }

            IsLoading = false;
            return true;
        }
    }
}
