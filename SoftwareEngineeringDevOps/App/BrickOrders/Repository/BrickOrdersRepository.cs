using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.BrickOrders.Persistence;
using SoftwareEngineeringDevOps.App.BrickOrders.Persistence.DBOs;
using SoftwareEngineeringDevOps.App.Users;
using SoftwareEngineeringDevOps.App.Users.Repository;

namespace SoftwareEngineeringDevOps.App.BrickOrders.Repository
{
    /// <summary>
    /// Sits between the mediator and the DB layer.
    /// Caches orders by id and resolves user info via IUsersRepository.
    /// </summary>
    public class BrickOrdersRepository : IBrickOrdersRepository
    {
        private readonly IBrickOrdersDB _db;
        private readonly IUsersRepository _usersRepository;
        private readonly ILogger<BrickOrdersRepository> _logger;
        private readonly Dictionary<long, IBrickOrder> _cacheById = [];

        public BrickOrdersRepository(IBrickOrdersDB db, IUsersRepository usersRepository, ILogger<BrickOrdersRepository> logger)
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

        public IEnumerable<IBrickOrder> ListAll()
        {
            try
            {
                IEnumerable<BrickOrder> orders = _db.ListAll()
                    .Select(dbo => BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById)))
                    .ToList();

                foreach (BrickOrder order in orders)
                {
                    _cacheById[order.Id] = order;
                }

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list all brick orders from repository");
                throw;
            }
        }

        public IBrickOrder? GetById(long id)
        {
            try
            {
                if (_cacheById.TryGetValue(id, out var cached))
                {
                    return cached;
                }

                BrickOrderDBO? dbo = _db.GetById(id);

                if (dbo == null)
                {
                    _logger.LogWarning("Brick order not found: {OrderId}", id);
                    return null;
                }

                BrickOrder order = BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
                _cacheById[order.Id] = order;
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get brick order by ID: {OrderId}", id);
                throw;
            }
        }

        public IEnumerable<IBrickOrder> GetByBrickId(long brickId)
        {
            try
            {
                IEnumerable<BrickOrder> orders = _db.GetByBrickId(brickId)
                    .Select(dbo => BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById)))
                    .ToList();

                foreach (BrickOrder order in orders)
                {
                    _cacheById[order.Id] = order;
                }

                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get brick orders by brick ID: {BrickId}", brickId);
                throw;
            }
        }

        public IBrickOrder Insert(NewBrickOrder brickOrder)
        {
            ArgumentNullException.ThrowIfNull(brickOrder);

            try
            {
                BrickOrderDBO dbo = _db.Insert(brickOrder);
                BrickOrder created = BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
                _cacheById[created.Id] = created;
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert brick order: Order No: {OrderNo}", brickOrder.OrderNo);
                throw;
            }
        }

        public IBrickOrder Update(EditBrickOrder brickOrder)
        {
            ArgumentNullException.ThrowIfNull(brickOrder);

            try
            {
                BrickOrderDBO dbo = _db.Update(brickOrder);
                BrickOrder updated = BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
                _cacheById[updated.Id] = updated;
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update brick order: {OrderId}", brickOrder.Id);
                throw;
            }
        }

        public IBrickOrder Cancel(long id, DateTime cancelledDate)
        {
            try
            {
                BrickOrderDBO dbo = _db.Cancel(id, cancelledDate);
                BrickOrder cancelled = BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
                _cacheById[cancelled.Id] = cancelled;
                return cancelled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel brick order: {OrderId}", id);
                throw;
            }
        }

        public IBrickOrder Uncancel(long id)
        {
            try
            {
                BrickOrderDBO dbo = _db.Uncancel(id);
                BrickOrder uncancelled = BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
                _cacheById[uncancelled.Id] = uncancelled;
                return uncancelled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to uncancel brick order: {OrderId}", id);
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
                _logger.LogError(ex, "Failed to delete brick order: {OrderId}", id);
                throw;
            }
        }
    }
}
