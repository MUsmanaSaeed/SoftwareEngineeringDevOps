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
        IBrickOrdersReceivedDB Db { get; }
        IUsersRepository UsersRepository { get; }
        readonly Dictionary<long, IBrickOrderReceived> _cacheById = [];

        public BrickOrdersReceivedRepository(IBrickOrdersReceivedDB db, IUsersRepository usersRepository)
        {
            Db = db;
            UsersRepository = usersRepository;
        }

        private IUserInfo GetUser(long id)
        {
            return UsersRepository.GetById(id)
                ?? throw new InvalidOperationException($"User {id} not found.");
        }

        public IEnumerable<IBrickOrderReceived> ListAll()
        {
            IEnumerable<BrickOrderReceived> records = Db.ListAll()
                .Select(dbo => BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById)))
                .ToList();
            foreach (BrickOrderReceived record in records)
            {
                _cacheById[record.Id] = record;
            }
            return records;
        }

        public IBrickOrderReceived? GetById(long id)
        {
            if (_cacheById.TryGetValue(id, out var cached)) return cached;
            BrickOrderReceivedDBO? dbo = Db.GetById(id);
            if (dbo == null) return null;
            BrickOrderReceived record = BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById));
            _cacheById[record.Id] = record;
            return record;
        }

        public IEnumerable<IBrickOrderReceived> GetByBrickOrderId(long brickOrderId)
        {
            IEnumerable<BrickOrderReceived> records = Db.GetByBrickOrderId(brickOrderId)
                .Select(dbo => BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById)))
                .ToList();
            foreach (BrickOrderReceived record in records)
            {
                _cacheById[record.Id] = record;
            }
            return records;
        }

        public IBrickOrderReceived Insert(NewBrickOrderReceived brickOrderReceived)
        {
            BrickOrderReceivedDBO dbo = Db.Insert(brickOrderReceived);
            BrickOrderReceived created = BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById));
            _cacheById[created.Id] = created;
            return created;
        }

        public IBrickOrderReceived Update(EditBrickOrderReceived brickOrderReceived)
        {
            BrickOrderReceivedDBO dbo = Db.Update(brickOrderReceived);
            BrickOrderReceived updated = BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById));
            _cacheById[updated.Id] = updated;
            return updated;
        }

        public void Delete(long id)
        {
            Db.Delete(id);
            _cacheById.Remove(id);
        }
    }
}
