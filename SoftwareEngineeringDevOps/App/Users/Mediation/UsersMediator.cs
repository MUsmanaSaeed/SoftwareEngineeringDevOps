using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.Users.Repository;

namespace SoftwareEngineeringDevOps.App.Users
{
    public class UsersMediator : IUsersMediator
    {
        private readonly IUsersRepository _repository;
        private readonly ILogger<UsersMediator> _logger;

        public UsersMediator(IUsersRepository repository, ILogger<UsersMediator> logger)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(logger);
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<IUser>> GetAllUsers()
        {
            try
            {
                await Task.CompletedTask;
                var users = _repository.ListAll();
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to get all users");
                throw;
            }
        }

        public async Task<IUser?> GetUserById(long id)
        {
            try
            {
                await Task.CompletedTask;
                var user = _repository.GetById(id);

                if (user == null)
                {
                    _logger.LogWarning("Mediator: User not found: {UserId}", id);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to get user by ID: {UserId}", id);
                throw;
            }
        }

        public async Task<IUser?> GetUserByUsername(string username)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(username);

            try
            {
                await Task.CompletedTask;
                var user = _repository.GetByUsername(username);

                if (user == null)
                {
                    _logger.LogWarning("Mediator: User not found by username: {Username}", username);
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to get user by username: {Username}", username);
                throw;
            }
        }

        public async Task<IUser> Insert(NewUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            try
            {
                await Task.CompletedTask;
                var created = _repository.Insert(user);
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to insert user: {Username}", user.Username);
                throw;
            }
        }

        public async Task<IUser> Update(EditUser user)
        {
            ArgumentNullException.ThrowIfNull(user);

            try
            {
                await Task.CompletedTask;
                var updated = _repository.Update(user);
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to update user: {UserId} - {Username}", user.Id, user.Username);
                throw;
            }
        }

        public async Task Delete(long id)
        {
            try
            {
                await Task.CompletedTask;
                _repository.Delete(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to delete user: {UserId}", id);
                throw;
            }
        }
    }
}
