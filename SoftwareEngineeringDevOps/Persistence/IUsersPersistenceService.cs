using SoftwareEngineeringDevOps.Models;

namespace SoftwareEngineeringDevOps.Persistence;

public interface IUsersPersistenceService
{
    Task<IEnumerable<User>> ListAllAsync();

    Task InsertAsync(
        string username,
        string password,
        string firstName,
        string secondName,
        bool isAdmin,
        bool isEditor);

    Task UpdateAsync(
        long id,
        string username,
        string password,
        string firstName,
        string secondName,
        bool isAdmin,
        bool isEditor);

    Task<User?> GetByUsernameAsync(string username);

    /// <summary>
    /// Validates credentials by comparing the supplied password against the stored value.
    /// Returns the User model on success, or null if credentials do not match.
    /// Password comparison is performed inside the persistence layer; the password is
    /// never exposed outside this layer.
    /// </summary>
    Task<User?> ValidateCredentialsAsync(string username, string password);

    Task DeleteAsync(long id);

    Task EnableAsync(long id);
}
