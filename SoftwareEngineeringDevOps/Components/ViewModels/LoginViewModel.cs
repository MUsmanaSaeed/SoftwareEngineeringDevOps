using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.Auth;

namespace SoftwareEngineeringDevOps.Components.ViewModels
{
    public class LoginViewModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<LoginViewModel> _logger;

        public LoginViewModel(IAuthService authService, ILogger<LoginViewModel> logger)
        {
            ArgumentNullException.ThrowIfNull(authService);
            ArgumentNullException.ThrowIfNull(logger);
            _authService = authService;
            _logger = logger;
        }

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public bool IsLoading { get; set; }
        public string? ReturnUrl { get; set; }

        public async Task<bool> ExecuteLogin()
        {
            try
            {
                ErrorMessage = null;
                IsLoading = true;

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "Username and password are required.";
                    _logger.LogWarning("ViewModel: Login attempt with missing credentials");
                    IsLoading = false;
                    return false;
                }

                var result = await _authService.LoginAsync(Username, Password);

                if (!result)
                {
                    ErrorMessage = "Invalid username or password.";
                    _logger.LogWarning("ViewModel: Login failed for username: {Username}", Username);
                    IsLoading = false;
                    return false;
                }

                IsLoading = false;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ViewModel: Error during login for username: {Username}", Username);
                ErrorMessage = "An error occurred during login. Please try again.";
                IsLoading = false;
                return false;
            }
        }
    }
}
