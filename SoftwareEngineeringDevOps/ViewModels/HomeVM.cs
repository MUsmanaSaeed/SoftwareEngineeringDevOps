using System.Security.Claims;
using SoftwareEngineeringDevOps.Auth;

namespace SoftwareEngineeringDevOps.ViewModels;

public sealed class HomeVM
{
    public string Username { get; private set; } = string.Empty;
    public bool IsAdmin { get; private set; }
    public bool IsEditor { get; private set; }

    /// <summary>
    /// Populates display properties from the authenticated user's claims.
    /// No database calls are made; all data is read from the encrypted cookie claims.
    /// </summary>
    public void Initialize(ClaimsPrincipal user)
    {
        Username = user.FindFirst(AppClaims.Username)?.Value ?? string.Empty;
        IsAdmin = string.Equals(user.FindFirst(AppClaims.IsAdmin)?.Value, "True", StringComparison.OrdinalIgnoreCase);
        IsEditor = string.Equals(user.FindFirst(AppClaims.IsEditor)?.Value, "True", StringComparison.OrdinalIgnoreCase);
    }
}
