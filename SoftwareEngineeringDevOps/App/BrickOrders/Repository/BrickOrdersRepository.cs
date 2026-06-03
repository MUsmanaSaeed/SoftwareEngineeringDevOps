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
        IBrickOrdersDB Db { get; }
        IUsersRepository UsersRepository { get; }
        readonly Dictionary<long, IBrickOrder> _cacheById = [];

        public BrickOrdersRepository(IBrickOrdersDB db, IUsersRepository usersRepository)
        {
            Db = db;
            UsersRepository = usersRepository;
        }

        private IUserInfo GetUser(long id)
        {
            return UsersRepository.GetById(id)
                ?? throw new InvalidOperationException($"User {id} not found.");
        }

        public IEnumerable<IBrickOrder> ListAll()
        {
            IEnumerable<BrickOrder> orders = Db.ListAll()
                .Select(dbo => BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById)))
                .ToList();
            foreach (BrickOrder order in orders)
            {
                _cacheById[order.Id] = order;
            }
            return orders;
        }

        public IBrickOrder? GetById(long id)
        {
            if (_cacheById.TryGetValue(id, out var cached)) return cached;
            BrickOrderDBO? dbo = Db.GetById(id);
            if (dbo == null) return null;
            BrickOrder order = BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
            _cacheById[order.Id] = order;
            return order;
        }

        public IEnumerable<IBrickOrder> GetByBrickId(long brickId)
        {
            IEnumerable<BrickOrder> orders = Db.GetByBrickId(brickId)
                .Select(dbo => BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById)))
                .ToList();
            foreach (BrickOrder order in orders)
            {
                _cacheById[order.Id] = order;
            }
            return orders;
        }

        public IBrickOrder Insert(NewBrickOrder brickOrder)
        {
            BrickOrderDBO dbo = Db.Insert(brickOrder);
            BrickOrder created = BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
            _cacheById[created.Id] = created;
            return created;
        }

        public IBrickOrder Update(EditBrickOrder brickOrder)
        {
            BrickOrderDBO dbo = Db.Update(brickOrder);
            BrickOrder updated = BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
            _cacheById[updated.Id] = updated;
            return updated;
        }

        public IBrickOrder Cancel(long id, DateTime cancelledDate)
        {
            BrickOrderDBO dbo = Db.Cancel(id, cancelledDate);
            BrickOrder cancelled = BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
            _cacheById[cancelled.Id] = cancelled;
            return cancelled;
        }

        public IBrickOrder Uncancel(long id)
        {
            BrickOrderDBO dbo = Db.Uncancel(id);
            BrickOrder uncancelled = BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
            _cacheById[uncancelled.Id] = uncancelled;
            return uncancelled;
        }

        public void Delete(long id)
        {
            Db.Delete(id);
            _cacheById.Remove(id);
        }
    }
}
