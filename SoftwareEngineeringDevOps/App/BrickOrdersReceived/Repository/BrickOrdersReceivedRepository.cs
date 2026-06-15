using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.BrickOrdersReceived.Persistence;
using SoftwareEngineeringDevOps.App.BrickOrdersReceived.Persistence.DBOs;
using SoftwareEngineeringDevOps.App.Users;
using SoftwareEngineeringDevOps.App.Users.Repository;

namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived.Repository
{
    /// <summary>
    /// Sits between the mediator and the DB layer.
    /// Caches received records by id and resolves user info via IUsersRepository.
    /// </summary>
    public class BrickOrdersReceivedRepository : IBrickOrdersReceivedRepository
    {
        private readonly IBrickOrdersReceivedDB _db;
        private readonly IUsersRepository _usersRepository;
        private readonly ILogger<BrickOrdersReceivedRepository> _logger;
        private readonly Dictionary<long, IBrickOrderReceived> _cacheById = [];

        public BrickOrdersReceivedRepository(IBrickOrdersReceivedDB db, IUsersRepository usersRepository, ILogger<BrickOrdersReceivedRepository> logger)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(usersRepository);
            ArgumentNullException.ThrowIfNull(logger);
            _db = db;
            _usersRepository = usersRepository;
            _logger = logger;
        }

        private IUserInfo GetUser(long id)
        {
            try
            {
                var user = _usersRepository.GetById(id);

                if (user == null)
                {
                    _logger.LogError("User not found: {UserId}", id);
                    throw new InvalidOperationException($"User {id} not found.");
                }

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve user: {UserId}", id);
                throw;
            }
        }

        public IEnumerable<IBrickOrderReceived> ListAll()
        {
            try
            {
                IEnumerable<BrickOrderReceived> records = _db.ListAll()
                    .Select(dbo => BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById)))
                    .ToList();

                foreach (BrickOrderReceived record in records)
                {
                    _cacheById[record.Id] = record;
                }

                return records;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list all brick orders received from repository");
                throw;
            }
        }

        public IBrickOrderReceived? GetById(long id)
        {
            try
            {
                if (_cacheById.TryGetValue(id, out var cached))
                {
                    return cached;
                }

                BrickOrderReceivedDBO? dbo = _db.GetById(id);

                if (dbo == null)
                {
                    _logger.LogWarning("Brick order received not found: {ReceivedId}", id);
                    return null;
                }

                BrickOrderReceived record = BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById));
                _cacheById[record.Id] = record;
                return record;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get brick order received by ID: {ReceivedId}", id);
                throw;
            }
        }

        public IEnumerable<IBrickOrderReceived> GetByBrickOrderId(long brickOrderId)
        {
            try
            {
                IEnumerable<BrickOrderReceived> records = _db.GetByBrickOrderId(brickOrderId)
                    .Select(dbo => BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById)))
                    .ToList();

                foreach (BrickOrderReceived record in records)
                {
                    _cacheById[record.Id] = record;
                }

                return records;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get brick orders received by order ID: {BrickOrderId}", brickOrderId);
                throw;
            }
        }

        public IBrickOrderReceived Insert(NewBrickOrderReceived brickOrderReceived)
        {
            ArgumentNullException.ThrowIfNull(brickOrderReceived);

            try
            {
                BrickOrderReceivedDBO dbo = _db.Insert(brickOrderReceived);
                BrickOrderReceived created = BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById));
                _cacheById[created.Id] = created;
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert brick order received: Order ID: {BrickOrderId}", 
                    brickOrderReceived.BrickOrderId);
                throw;
            }
        }

        public IBrickOrderReceived Update(EditBrickOrderReceived brickOrderReceived)
        {
            ArgumentNullException.ThrowIfNull(brickOrderReceived);

            try
            {
                BrickOrderReceivedDBO dbo = _db.Update(brickOrderReceived);
                BrickOrderReceived updated = BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById));
                _cacheById[updated.Id] = updated;
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update brick order received: {ReceivedId}", brickOrderReceived.Id);
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
                _logger.LogError(ex, "Failed to delete brick order received: {ReceivedId}", id);
                throw;
            }
        }
    }
}
