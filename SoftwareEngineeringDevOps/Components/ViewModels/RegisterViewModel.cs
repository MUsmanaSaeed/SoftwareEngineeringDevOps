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
        public List<string> ValidationErrors { get; set; } = new();
        public bool IsLoading { get; set; }

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
            await _authService.Login(Username, Password);
            IsLoading = false;
            return true;
        }
    }
}
