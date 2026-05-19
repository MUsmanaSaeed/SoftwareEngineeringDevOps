using System.Security.Claims;
using SoftwareEngineeringDevOps.Auth;
using SoftwareEngineeringDevOps.Models;
using SoftwareEngineeringDevOps.Persistence;

namespace SoftwareEngineeringDevOps.ViewModels;

public sealed class UserProfileVM
{
    private readonly IUsersPersistenceService _persistence;

    private long _userId;

    // Profile form fields
    public string AuthenticatedUsername { get; private set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string SecondName { get; set; } = string.Empty;
    public bool IsAdmin { get; private set; }
    public bool IsEditor { get; private set; }
    public bool IsDeleted { get; private set; }

    // Password change fields
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;

    public string? ErrorMessage { get; private set; }
    public string? SuccessMessage { get; private set; }
    public bool IsLoaded { get; private set; }

    public UserProfileVM(IUsersPersistenceService persistence)
    {
        _persistence = persistence;
    }

    /// <summary>
    /// Loads the authenticated user's record from the database using the username claim.
    /// Standard users can only access their own record.
    /// </summary>
    public async Task InitializeAsync(ClaimsPrincipal principal)
    {
        ErrorMessage = null;
        SuccessMessage = null;

        AuthenticatedUsername = principal.FindFirst(AppClaims.Username)?.Value ?? string.Empty;

        var user = await _persistence.GetByUsernameAsync(AuthenticatedUsername);

        if (user is null)
        {
            ErrorMessage = "User record could not be found.";
            return;
        }

        PopulateFromUser(user);
        IsLoaded = true;
    }

    /// <summary>
    /// Updates the user's own profile (username, first name, second name).
    /// Requires the current password for identity confirmation.
    /// IsAdmin and IsEditor are preserved from the current record and cannot be self-modified.
    /// </summary>
    public async Task SaveProfileAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (!ValidateProfileFields())
            return;

        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            ErrorMessage = "Current password is required to save changes.";
            return;
        }

        var validatedUser = await _persistence.ValidateCredentialsAsync(AuthenticatedUsername, CurrentPassword);

        if (validatedUser is null)
        {
            ErrorMessage = "Incorrect current password.";
            return;
        }

        await _persistence.UpdateAsync(
            _userId,
            Username.Trim(),
            CurrentPassword,
            FirstName.Trim(),
            SecondName.Trim(),
            IsAdmin,
            IsEditor);

        AuthenticatedUsername = Username.Trim();
        SuccessMessage = "Profile updated successfully.";
        CurrentPassword = string.Empty;
    }

    /// <summary>
    /// Changes the authenticated user's password after validating the current password,
    /// checking password strength, and confirming the new password matches.
    /// </summary>
    public async Task ChangePasswordAsync()
    {
        ErrorMessage = null;
        SuccessMessage = null;

        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            ErrorMessage = "Current password is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NewPassword))
        {
            ErrorMessage = "New password is required.";
            return;
        }

        if (!PasswordValidator.IsStrong(NewPassword))
        {
            ErrorMessage = PasswordValidator.StrengthMessage;
            return;
        }

        if (NewPassword != ConfirmPassword)
        {
            ErrorMessage = "New password and confirmation do not match.";
            return;
        }

        var validatedUser = await _persistence.ValidateCredentialsAsync(AuthenticatedUsername, CurrentPassword);

        if (validatedUser is null)
        {
            ErrorMessage = "Incorrect current password.";
            return;
        }

        await _persistence.UpdateAsync(
            _userId,
            Username.Trim(),
            NewPassword,
            FirstName.Trim(),
            SecondName.Trim(),
            IsAdmin,
            IsEditor);

        SuccessMessage = "Password changed successfully.";
        CurrentPassword = string.Empty;
        NewPassword = string.Empty;
        ConfirmPassword = string.Empty;
    }

    private void PopulateFromUser(User user)
    {
        _userId = user.Id;
        Username = user.Username;
        FirstName = user.FirstName;
        SecondName = user.SecondName;
        IsAdmin = user.IsAdmin;
        IsEditor = user.IsEditor;
        IsDeleted = user.IsDeleted;
    }

    private bool ValidateProfileFields()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "Username is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(FirstName))
        {
            ErrorMessage = "First name is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(SecondName))
        {
            ErrorMessage = "Second name is required.";
            return false;
        }

        return true;
    }
}
