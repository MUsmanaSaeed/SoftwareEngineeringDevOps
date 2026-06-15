using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.Auth;
using SoftwareEngineeringDevOps.App.Users;
using SoftwareEngineeringDevOps.App.Validation;

namespace SoftwareEngineeringDevOps.Components.ViewModels
{
    public class RegisterViewModel
    {
        private readonly IUsersMediator _usersMediator;
        private readonly IAuthService _authService;
        private readonly ILogger<RegisterViewModel> _logger;

        public RegisterViewModel(IUsersMediator usersMediator, IAuthService authService, ILogger<RegisterViewModel> logger)
        {
            ArgumentNullException.ThrowIfNull(usersMediator);
            ArgumentNullException.ThrowIfNull(authService);
            ArgumentNullException.ThrowIfNull(logger);
            _usersMediator = usersMediator;
            _authService = authService;
            _logger = logger;
        }

        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public List<string> ValidationErrors { get; set; } = [];
        public bool IsLoading { get; set; }

        /// <summary>Tracks whether the user has manually edited the username field.</summary>
        public bool IsUsernameManuallyEdited { get; set; }

        /// <summary>
        /// Generates a unique username from first + last name and updates <see cref="Username"/>
        /// unless the user has already manually edited the field.
        /// </summary>
        public async Task GenerateUsernameAsync()
        {
            try
            {
                if (IsUsernameManuallyEdited) return;

                var firstName = FirstName.Trim();
                var lastName = LastName.Trim();

                if (string.IsNullOrWhiteSpace(firstName) && string.IsNullOrWhiteSpace(lastName))
                {
                    Username = string.Empty;
                    return;
                }

                var baseUsername = $"{firstName}{lastName}"
                    .ToLowerInvariant()
                    .Replace(" ", string.Empty);

                var candidate = baseUsername;
                var existing = await _usersMediator.GetUserByUsername(candidate);

                if (existing != null)
                {
                    int suffix = 2;
                    do
                    {
                        candidate = $"{baseUsername}{suffix:D2}";
                        existing = await _usersMediator.GetUserByUsername(candidate);
                        suffix++;
                    }
                    while (existing != null);
                }

                Username = candidate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ViewModel: Error generating username");
            }
        }

        public async Task<bool> ExecuteRegister()
        {
            try
            {
                ErrorMessage = null;
                ValidationErrors.Clear();
                IsLoading = true;

                var newUser = new NewUser
                {
                    Username = Username,
                    Password = Password,
                    FirstName = FirstName,
                    LastName = LastName,
                    IsAdmin = false,
                    IsEditor = false
                };

                var errors = InputValidator.ValidateUser(newUser);
                if (errors.Count > 0)
                {
                    ValidationErrors = errors;
                    _logger.LogWarning("ViewModel: Registration validation failed for username: {Username}", Username);
                    IsLoading = false;
                    return false;
                }

                var existing = await _usersMediator.GetUserByUsername(Username);
                if (existing != null)
                {
                    ErrorMessage = "Username already exists.";
                    _logger.LogWarning("ViewModel: Registration failed - username already exists: {Username}", Username);
                    IsLoading = false;
                    return false;
                }

                await _usersMediator.Insert(newUser);

                await _authService.LoginAsync(Username, Password);

                IsLoading = false;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ViewModel: Error during registration for username: {Username}", Username);
                ErrorMessage = "An error occurred during registration. Please try again.";
                IsLoading = false;
                return false;
            }
        }
    }
}
