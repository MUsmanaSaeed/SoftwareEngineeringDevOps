using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.Auth;
using SoftwareEngineeringDevOps.App.Users;
using SoftwareEngineeringDevOps.App.Validation;

namespace SoftwareEngineeringDevOps.Components.ViewModels
{
    public class UsersViewModel
    {
        private readonly IUsersMediator _usersMediator;
        private readonly IAuthService _authService;
        private readonly ILogger<UsersViewModel> _logger;

        public UsersViewModel(IUsersMediator usersMediator, IAuthService authService, ILogger<UsersViewModel> logger)
        {
            ArgumentNullException.ThrowIfNull(usersMediator);
            ArgumentNullException.ThrowIfNull(authService);
            ArgumentNullException.ThrowIfNull(logger);
            _usersMediator = usersMediator;
            _authService = authService;
            _logger = logger;
        }

        public IEnumerable<IUser> Users { get; set; } = Enumerable.Empty<IUser>();
        public IUser? SelectedUser { get; set; }
        public NewUser NewUserModel { get; set; } = new();
        public EditUser? EditUserModel { get; set; }
        public bool IsLoading { get; set; }
        public bool ShowAddModal { get; set; }
        public bool ShowEditModal { get; set; }
        public bool ShowDeleteConfirm { get; set; }
        public List<string> ValidationErrors { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public UserRole CurrentUserRole => _authService.CurrentUser != null
            ? RoleHelper.GetRole(_authService.CurrentUser)
            : UserRole.Standard;

        public long CurrentUserId => _authService.CurrentUser?.Id ?? 0;
        public string UsersSearchTerm { get; set; } = string.Empty;

        public IEnumerable<IUser> FilteredUsers =>
            Users.Where(user =>
                string.IsNullOrWhiteSpace(UsersSearchTerm)
                || user.Username.Contains(UsersSearchTerm, StringComparison.OrdinalIgnoreCase)
                || $"{user.FirstName} {user.LastName}".Contains(UsersSearchTerm, StringComparison.OrdinalIgnoreCase)
                || (user.IsAdmin ? "Admin" : user.IsEditor ? "Editor" : "Standard").Contains(UsersSearchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderBy(user => user.Username, StringComparer.OrdinalIgnoreCase);

        public async Task LoadUsers()
        {
            try
            {
                IsLoading = true;
                Users = await _usersMediator.GetAllUsers();
                IsLoading = false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ViewModel: Error loading users");
                ErrorMessage = "Failed to load users. Please try again.";
                IsLoading = false;
            }
        }

        public void SelectUser(IUser user)
        {
            SelectedUser = user;
        }

        public void OpenAddModal()
        {
            NewUserModel = new NewUser();
            ValidationErrors.Clear();
            ShowAddModal = true;
        }

        public void CloseAddModal()
        {
            ShowAddModal = false;
            ValidationErrors.Clear();
        }

        public async Task<bool> AddUser()
        {
            try
            {
                ValidationErrors.Clear();
                var errors = InputValidator.ValidateUser(NewUserModel, requireLastName: false, requireStrongPassword: false);
                if (errors.Count > 0)
                {
                    ValidationErrors = errors;
                    _logger.LogWarning("ViewModel: User validation failed for new user: {Username}", NewUserModel.Username);
                    ShowAddModal = true;
                    return false;
                }

                if (Users.Any(u => u.Username.Equals(NewUserModel.Username, StringComparison.OrdinalIgnoreCase)))
                {
                    ValidationErrors.Add("A user with that username already exists.");
                    _logger.LogWarning("ViewModel: Duplicate username detected: {Username}", NewUserModel.Username);
                    ShowAddModal = true;
                    return false;
                }

                IsLoading = true;
                await _usersMediator.Insert(NewUserModel);
                await LoadUsers();
                ShowAddModal = false;
                IsLoading = false;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ViewModel: Error adding user: {Username}", NewUserModel.Username);
                ErrorMessage = "Failed to add user. Please try again.";
                IsLoading = false;
                ShowAddModal = false;
                return false;
            }
        }

        public void OpenEditModal(IUser user)
        {
            EditUserModel = new EditUser(user);
            ValidationErrors.Clear();
            ShowEditModal = true;
        }

        public void CloseEditModal()
        {
            ShowEditModal = false;
            ValidationErrors.Clear();
        }

        public async Task<bool> UpdateUser()
        {
            try
            {
                if (EditUserModel == null) return false;

                ValidationErrors.Clear();
                var errors = InputValidator.ValidateUser(EditUserModel, requireLastName: false, requireStrongPassword: false);
                if (errors.Count > 0)
                {
                    ValidationErrors = errors;
                    _logger.LogWarning("ViewModel: User validation failed for edit user: {UserId}", EditUserModel.Id);
                    ShowEditModal = true;
                    return false;
                }

                if (Users.Any(u => u.Id != EditUserModel.Id && u.Username.Equals(EditUserModel.Username, StringComparison.OrdinalIgnoreCase)))
                {
                    ValidationErrors.Add("A user with that username already exists.");
                    _logger.LogWarning("ViewModel: Duplicate username detected during edit: {Username}", EditUserModel.Username);
                    ShowEditModal = true;
                    return false;
                }

                // Self-preservation: prevent admin from removing their own admin status
                if (EditUserModel.Id == CurrentUserId && !EditUserModel.IsAdmin)
                {
                    ValidationErrors.Add("You cannot remove your own admin privileges.");
                    _logger.LogWarning("ViewModel: Attempted to remove own admin privileges: {UserId}", EditUserModel.Id);
                    ShowEditModal = true;
                    return false;
                }

                IsLoading = true;
                await _usersMediator.Update(EditUserModel);
                await LoadUsers();
                ShowEditModal = false;

                if (SelectedUser?.Id == EditUserModel.Id)
                {
                    SelectedUser = (await _usersMediator.GetUserById(EditUserModel.Id)) as IUser;
                }
                IsLoading = false;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ViewModel: Error updating user: {UserId}", EditUserModel?.Id);
                ErrorMessage = "Failed to update user. Please try again.";
                IsLoading = false;
                ShowEditModal = false;
                return false;
            }
        }

        public void OpenDeleteConfirm(IUser user)
        {
            SelectedUser = user;
            ErrorMessage = null;
            ShowDeleteConfirm = true;
        }

        public void CloseDeleteConfirm()
        {
            ShowDeleteConfirm = false;
            ErrorMessage = null;
        }

        public async Task<bool> DeleteUser()
        {
            try
            {
                if (SelectedUser == null) return false;

                // Self-preservation: prevent admin from deleting themselves
                if (SelectedUser.Id == CurrentUserId)
                {
                    ErrorMessage = "You cannot delete your own account.";
                    _logger.LogWarning("ViewModel: Attempted to delete own account: {UserId}", SelectedUser.Id);
                    return false;
                }

                IsLoading = true;
                await _usersMediator.Delete(SelectedUser.Id);
                SelectedUser = null;
                await LoadUsers();
                ShowDeleteConfirm = false;
                IsLoading = false;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ViewModel: Error deleting user: {UserId}", SelectedUser?.Id);
                ErrorMessage = "Failed to delete user. Please try again.";
                IsLoading = false;
                ShowDeleteConfirm = false;
                return false;
            }
        }
    }
}
