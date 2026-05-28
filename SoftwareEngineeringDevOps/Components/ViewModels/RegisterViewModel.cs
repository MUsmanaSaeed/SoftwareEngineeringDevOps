using SoftwareEngineeringDevOps.App.Auth;
using SoftwareEngineeringDevOps.App.Users;
using SoftwareEngineeringDevOps.App.Validation;

namespace SoftwareEngineeringDevOps.Components.ViewModels
{
    public class RegisterViewModel
    {
        private readonly IUsersMediator _usersMediator;
        private readonly IAuthService _authService;

        public RegisterViewModel(IUsersMediator usersMediator, IAuthService authService)
        {
            _usersMediator = usersMediator;
            _authService = authService;
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

        public async Task<bool> ExecuteRegister()
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
                IsLoading = false;
                return false;
            }

            var existing = await _usersMediator.GetUserByUsername(Username);
            if (existing != null)
            {
                ErrorMessage = "Username already exists.";
                IsLoading = false;
                return false;
            }

            await _usersMediator.Insert(newUser);
            await _authService.LoginAsync(Username, Password);
            IsLoading = false;
            return true;
        }
    }
}
