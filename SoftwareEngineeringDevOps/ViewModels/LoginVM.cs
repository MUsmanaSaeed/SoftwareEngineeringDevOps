using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SoftwareEngineeringDevOps.Auth;
using SoftwareEngineeringDevOps.Persistence;

namespace SoftwareEngineeringDevOps.ViewModels;

public sealed class LoginVM
{
    private readonly IUsersPersistenceService _persistence;

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ErrorMessage { get; private set; }

    public LoginVM(IUsersPersistenceService persistence)
    {
        _persistence = persistence;
    }

    /// <summary>
    /// Validates credentials and, on success, issues the authentication cookie.
    /// Returns true on success so the caller can redirect.
    /// The password is used only during this call and is never stored beyond it.
    /// </summary>
    public async Task<bool> LoginAsync(string username, string password, HttpContext httpContext)
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Username and password are required.";
            return false;
        }

        var user = await _persistence.ValidateCredentialsAsync(username, password);

        if (user is null)
        {
            ErrorMessage = "Invalid username or password.";
            return false;
        }

        if (user.IsDeleted)
        {
            ErrorMessage = "This account has been disabled. Please contact an administrator.";
            return false;
        }

        var claims = new List<Claim>
        {
            new(AppClaims.Username, user.Username),
            new(AppClaims.IsAdmin, user.IsAdmin.ToString()),
            new(AppClaims.IsEditor, user.IsEditor.ToString()),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                IssuedUtc = DateTimeOffset.UtcNow,
            });

        return true;
    }
}
