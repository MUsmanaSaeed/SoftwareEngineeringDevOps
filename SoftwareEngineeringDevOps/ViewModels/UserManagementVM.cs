using System.Security.Claims;
using System.Text.RegularExpressions;
using SoftwareEngineeringDevOps.Auth;
using SoftwareEngineeringDevOps.Models;
using SoftwareEngineeringDevOps.Persistence;

namespace SoftwareEngineeringDevOps.ViewModels;

public sealed class UserManagementVM
{
    private readonly IUsersPersistenceService _persistence;

    private string _adminUsername = string.Empty;
    private long _adminId;

    public IReadOnlyList<User> Users { get; private set; } = [];
    public bool IsAccessDenied { get; private set; }
    public bool IsAddMode { get; private set; }
    public User? SelectedUser { get; private set; }

    // Form fields for add/edit
    public string FormUsername { get; set; } = string.Empty;
    public string FormPassword { get; set; } = string.Empty;
    public string FormFirstName { get; set; } = string.Empty;
    public string FormSecondName { get; set; } = string.Empty;
    public bool FormIsAdmin { get; set; }
    public bool FormIsEditor { get; set; }

    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }

    public UserManagementVM(IUsersPersistenceService persistence)
    {
        _persistence = persistence;
    }

    /// <summary>
    /// Initialises the ViewModel. Enforces that only admins may access this page.
    /// Loads all users and resolves the current admin's record to obtain their Id
    /// for self-restriction checks.
    /// </summary>
    public async Task InitializeAsync(ClaimsPrincipal principal)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        var isAdmin = string.Equals(
            principal.FindFirst(AppClaims.IsAdmin)?.Value,
            "True",
            StringComparison.OrdinalIgnoreCase);

        if (!isAdmin)
        {
            IsAccessDenied = true;
            return;
        }

        _adminUsername = principal.FindFirst(AppClaims.Username)?.Value ?? string.Empty;

        var adminUser = await _persistence.GetByUsernameAsync(_adminUsername);
        _adminId = adminUser?.Id ?? 0;

        await RefreshUsersAsync();
    }

    public void PrepareForAdd()
    {
        SelectedUser = null;
        IsAddMode = true;
        ClearForm();
        ErrorMessage = null;
        SuccessMessage = null;
    }

    public void SelectForEdit(User user)
    {
        SelectedUser = user;
        IsAddMode = false;
        FormUsername = user.Username;
        FormPassword = string.Empty;
        FormFirstName = user.FirstName;
        FormSecondName = user.SecondName;
        FormIsAdmin = user.IsAdmin;
        FormIsEditor = user.IsEditor;
        ErrorMessage = null;
        SuccessMessage = null;
    }

    public void CancelForm()
    {
        SelectedUser = null;
        IsAddMode = false;
        ClearForm();
        ErrorMessage = null;
        SuccessMessage = null;
    }

    /// <summary>
    /// Saves the form: calls users_insert for new users or users_update for existing ones.
    /// Enforces that an admin cannot change their own IsAdmin flag.
    /// Password is required for both insert and update operations.
    /// </summary>
    public async Task SaveAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (!ValidateForm())
            return;

        if (IsAddMode)
        {
            await _persistence.InsertAsync(
                FormUsername.Trim(),
                FormPassword,
                FormFirstName.Trim(),
                FormSecondName.Trim(),
                FormIsAdmin,
                FormIsEditor);

            SuccessMessage = $"User '{FormUsername.Trim()}' created successfully.";
            ClearForm();
            IsAddMode = false;
        }
        else
        {
            if (SelectedUser is null)
                return;

            var targetIsAdmin = FormIsAdmin;

            // Admins cannot change their own IsAdmin flag
            if (SelectedUser.Id == _adminId)
                targetIsAdmin = SelectedUser.IsAdmin;

            await _persistence.UpdateAsync(
                SelectedUser.Id,
                FormUsername.Trim(),
                FormPassword,
                FormFirstName.Trim(),
                FormSecondName.Trim(),
                targetIsAdmin,
                FormIsEditor);

            SuccessMessage = $"User '{FormUsername.Trim()}' updated successfully.";
            SelectedUser = null;
            ClearForm();
        }

        await RefreshUsersAsync();
    }

    /// <summary>
    /// Soft-deletes a user by Id via users_delete.
    /// Admins cannot delete themselves.
    /// </summary>
    public async Task DeleteAsync(long id)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (id == _adminId)
        {
            ErrorMessage = "You cannot delete your own account.";
            return;
        }

        await _persistence.DeleteAsync(id);
        SuccessMessage = "User disabled successfully.";
        await RefreshUsersAsync();
    }

    /// <summary>
    /// Re-enables a previously deleted user by Id via users_enable.
    /// </summary>
    public async Task EnableAsync(long id)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        await _persistence.EnableAsync(id);
        SuccessMessage = "User enabled successfully.";
        await RefreshUsersAsync();
    }

    /// <summary>
    /// Returns true if the target user is the currently authenticated admin,
    /// so the UI can disable the IsAdmin checkbox for self-editing.
    /// </summary>
    public bool IsCurrentAdmin(User user) => user.Id == _adminId;

    private async Task RefreshUsersAsync()
    {
        var users = await _persistence.ListAllAsync();
        Users = users.ToList();
    }

    private bool ValidateForm()
    {
        if (string.IsNullOrWhiteSpace(FormUsername))
        {
            ErrorMessage = "Username is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(FormPassword))
        {
            ErrorMessage = "Password is required.";
            return false;
        }

        if (!IsPasswordStrong(FormPassword))
        {
            ErrorMessage = "Password must be at least 8 characters and include an uppercase letter, a digit, and a special character.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(FormFirstName))
        {
            ErrorMessage = "First name is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(FormSecondName))
        {
            ErrorMessage = "Second name is required.";
            return false;
        }

        return true;
    }

    private void ClearForm()
    {
        FormUsername = string.Empty;
        FormPassword = string.Empty;
        FormFirstName = string.Empty;
        FormSecondName = string.Empty;
        FormIsAdmin = false;
        FormIsEditor = false;
    }

    private static bool IsPasswordStrong(string password) =>
        password.Length >= 8
        && Regex.IsMatch(password, "[A-Z]")
        && Regex.IsMatch(password, "[0-9]")
        && Regex.IsMatch(password, @"[^a-zA-Z0-9]");
}
