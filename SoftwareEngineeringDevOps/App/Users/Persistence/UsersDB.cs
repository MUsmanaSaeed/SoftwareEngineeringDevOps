using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.Database;
using SoftwareEngineeringDevOps.App.Users.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.Users.Persistence
{
    public class UsersDB : DB, IUsersDB
    {
        private readonly ILogger<UsersDB> _logger;

        public UsersDB(SQL_Execute sqlExecute, ILogger<UsersDB> logger) : base(sqlExecute, logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
        }

        public IEnumerable<UserDBO> ListAll()
        {
            try
            {
                var users = Select<UserDBO>("users_listall") ?? [];
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all users");
                throw;
            }
        }

        public UserDBO? GetById(long id)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Id", id },
                };

                UserDBO? user = Select<UserDBO>("users_getbyid", parameters)?.FirstOrDefault();

                if (user == null)
                {
                    _logger.LogWarning("User not found with ID: {UserId}", id);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user by ID: {UserId}", id);
                throw;
            }
        }

        public UserDBO? GetByUsername(string username)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);

            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Username", username },
                };

                UserDBO? user = Select<UserDBO>("users_getbyusername", parameters)?.FirstOrDefault();

                if (user == null)
                {
                    _logger.LogWarning("User not found with username: {Username}", username);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve user by username: {Username}", username);
                throw;
            }
        }

        public UserDBO Insert(NewUser user)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentException.ThrowIfNullOrWhiteSpace(user.Username);
            ArgumentException.ThrowIfNullOrWhiteSpace(user.FirstName);

            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Username", user.Username },
                    { "Password", user.Password },
                    { "FirstName", user.FirstName },
                    { "LastName", user.LastName ?? string.Empty },
                    { "IsAdmin", user.IsAdmin },
                    { "IsEditor", user.IsEditor },
                };

                UserDBO? insertedUser = Select<UserDBO>("users_insert", parameters)?.FirstOrDefault();

                if (insertedUser == null)
                {
                    _logger.LogError("Failed to insert user - database returned null: {Username}", user.Username);
                    throw new InvalidOperationException($"Failed to insert user: {user.Username}");
                }

                return insertedUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert user: {Username}", user.Username);
                throw;
            }
        }

        public UserDBO Update(EditUser user)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentException.ThrowIfNullOrWhiteSpace(user.Username);
            ArgumentException.ThrowIfNullOrWhiteSpace(user.FirstName);

            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Id", user.Id },
                    { "Username", user.Username },
                    { "Password", user.Password },
                    { "FirstName", user.FirstName },
                    { "LastName", user.LastName ?? string.Empty },
                    { "IsAdmin", user.IsAdmin },
                    { "IsEditor", user.IsEditor },
                };

                UserDBO? updatedUser = Select<UserDBO>("users_update", parameters)?.FirstOrDefault();

                if (updatedUser == null)
                {
                    _logger.LogError("Failed to update user - database returned null: {UserId} - {Username}", 
                        user.Id, user.Username);
                    throw new InvalidOperationException($"Failed to update user: {user.Id}");
                }

                return updatedUser;
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
                Dictionary<string, object?> parameters = new()
                {
                    { "Id", id },
                };

                ExecuteWithParameters("users_delete", parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete user: {UserId}", id);
                throw;
            }
        }
    }
}
