using SoftwareEngineeringDevOps.App.Auth;
using SoftwareEngineeringDevOps.App.Users;
using SoftwareEngineeringDevOps.App.Validation;

namespace SoftwareEngineeringDevOps.Components.ViewModels
{
    public class UsersViewModel
    {
        private readonly IUsersMediator _usersMediator;
        private readonly IAuthService _authService;

        public UsersViewModel(IUsersMediator usersMediator, IAuthService authService)
        {
            _usersMediator = usersMediator;
            _authService = authService;
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

        public async Task LoadUsers()
        {
            IsLoading = true;
            Users = await _usersMediator.GetAllUsers();
            IsLoading = false;
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
            ValidationErrors.Clear();
            var errors = InputValidator.ValidateUser(NewUserModel);
            if (errors.Count > 0)
            {
                ValidationErrors = errors;
                return false;
            }

            IsLoading = true;
            await _usersMediator.Insert(NewUserModel);
            await LoadUsers();
            ShowAddModal = false;
            IsLoading = false;
            return true;
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
            if (EditUserModel == null) return false;

            ValidationErrors.Clear();
            var errors = InputValidator.ValidateUser(EditUserModel);
            if (errors.Count > 0)
            {
                ValidationErrors = errors;
                return false;
            }

            // Self-preservation: prevent admin from removing their own admin status
            if (EditUserModel.Id == CurrentUserId && !EditUserModel.IsAdmin)
            {
                ValidationErrors.Add("You cannot remove your own admin privileges.");
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
            if (SelectedUser == null) return false;

            // Self-preservation: prevent admin from deleting themselves
            if (SelectedUser.Id == CurrentUserId)
            {
                ErrorMessage = "You cannot delete your own account.";
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
    }
}
