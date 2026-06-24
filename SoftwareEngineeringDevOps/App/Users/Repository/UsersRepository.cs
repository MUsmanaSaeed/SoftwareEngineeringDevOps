using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.Users.Persistence;
using SoftwareEngineeringDevOps.App.Users.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.Users.Repository
{
    /// <summary>
    /// Sits between the mediator and the DB layer.
    /// Caches users by id so repeated lookups (e.g. hydrating received records)
    /// never hit the database more than once per id per application lifetime.
    /// </summary>
    public class UsersRepository : IUsersRepository
    {
        private readonly IUsersDB _db;
        private readonly ILogger<UsersRepository> _logger;
        private readonly Dictionary<long, IUser> _cacheById = [];

        public UsersRepository(IUsersDB db, ILogger<UsersRepository> logger)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(logger);
            _db = db;
            _logger = logger;
        }

        public IEnumerable<IUser> ListAll()
        {
            try
            {
                IEnumerable<User> users = _db.ListAll().Select(User.FromDBO).ToList();

                foreach (User user in users)
                {
                    _cacheById[user.Id] = user;
                }

                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list all users from repository");
                throw;
            }
        }

        public IUser? GetById(long id)
        {
            try
            {
                if (_cacheById.TryGetValue(id, out var cached))
                {
                    return cached;
                }

                UserDBO? dbo = _db.GetById(id);

                if (dbo == null)
                {
                    _logger.LogWarning("User not found: {UserId}", id);
                    return null;
                }

                User user = User.FromDBO(dbo);
                _cacheById[user.Id] = user;
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user by ID: {UserId}", id);
                throw;
            }
        }

        public IUser? GetByUsername(string username)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);

            try
            {
                IUser? existing = _cacheById.Values.FirstOrDefault(u =>
                    string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    return existing;
                }

                UserDBO? dbo = _db.GetByUsername(username);

                if (dbo == null)
                {
                    _logger.LogWarning("User not found by username: {Username}", username);
                    return null;
                }

                User user = User.FromDBO(dbo);
                _cacheById[user.Id] = user;
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user by username: {Username}", username);
                throw;
            }
        }

        public IUser Insert(NewUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            try
            {
                UserDBO dbo = _db.Insert(user);
                User created = User.FromDBO(dbo);
                _cacheById[created.Id] = created;
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert user: {Username}", user.Username);
                throw;
            }
        }

        public IUser Update(EditUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            try
            {
                UserDBO dbo = _db.Update(user);
                User updated = User.FromDBO(dbo);
                _cacheById[updated.Id] = updated;
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update user: {UserId} - {Username}", user.Id, user.Username);
                throw;
            }
        }

        public void Delete(long id)
        {
            try
            {
                _db.Delete(id);
                _cacheById.Remove(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user: {UserId}", id);
                throw;
            }
        }
    }
}
