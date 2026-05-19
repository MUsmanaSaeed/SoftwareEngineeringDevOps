using System.Text.RegularExpressions;

namespace SoftwareEngineeringDevOps.Auth;

/// <summary>
/// Shared password validation utility. Centralises the password policy so that both
/// the user profile and admin user management pages enforce the same rules.
/// </summary>
public static partial class PasswordValidator
{
    [GeneratedRegex("[A-Z]")]
    private static partial Regex UppercaseRegex();

    [GeneratedRegex("[0-9]")]
    private static partial Regex DigitRegex();

    [GeneratedRegex(@"[^a-zA-Z0-9]")]
    private static partial Regex SpecialCharRegex();

    /// <summary>
    /// Returns true when the password meets the minimum strength requirements:
    /// at least 8 characters, one uppercase letter, one digit, and one special character.
    /// </summary>
    public static bool IsStrong(string password) =>
        password.Length >= 8
        && UppercaseRegex().IsMatch(password)
        && DigitRegex().IsMatch(password)
        && SpecialCharRegex().IsMatch(password);

    public const string StrengthMessage =
        "Password must be at least 8 characters and include an uppercase letter, a digit, and a special character.";
}
