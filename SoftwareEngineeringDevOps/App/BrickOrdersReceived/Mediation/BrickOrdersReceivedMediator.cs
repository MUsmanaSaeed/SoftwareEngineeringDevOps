using SoftwareEngineeringDevOps.App.BrickOrdersReceived.Persistence;
using SoftwareEngineeringDevOps.App.Users;

namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived
{
    public class BrickOrdersReceivedMediator : IBrickOrdersReceivedMediator
    {
        IBrickOrdersReceivedDB Db { get; }
        IUsersMediator UsersMediator { get; }

        public BrickOrdersReceivedMediator(IBrickOrdersReceivedDB db, IUsersMediator usersMediator)
        {
            Db = db;
            UsersMediator = usersMediator;
        }

        private IUserInfo GetUser(long id)
        {
            return UsersMediator.GetUserById(id).GetAwaiter().GetResult()
                ?? throw new InvalidOperationException($"User {id} not found.");
        }

        public async Task<IEnumerable<IBrickOrderReceived>> GetAllBrickOrdersReceived()
        {
            await Task.CompletedTask;
            return Db.ListAll().Select(dbo => BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById)));
        }

        public async Task<IBrickOrderReceived?> GetBrickOrderReceivedById(long id)
        {
            await Task.CompletedTask;
            var dbo = Db.GetById(id);
            return dbo == null ? null : BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById));
        }

        public async Task<IEnumerable<IBrickOrderReceived>> GetBrickOrdersReceivedByBrickOrderId(long brickOrderId)
        {
            await Task.CompletedTask;
            return Db.GetByBrickOrderId(brickOrderId).Select(dbo => BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById)));
        }

        public async Task<IBrickOrderReceived> Insert(NewBrickOrderReceived brickOrderReceived)
        {
            await Task.CompletedTask;
            var dbo = Db.Insert(brickOrderReceived);
            return BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById));
        }

        public async Task<IBrickOrderReceived> Update(EditBrickOrderReceived brickOrderReceived)
        {
            await Task.CompletedTask;
            var dbo = Db.Update(brickOrderReceived);
            return BrickOrderReceived.FromDBO(dbo, GetUser(dbo.ReceivedById));
        }

        public async Task Delete(long id)
        {
            await Task.CompletedTask;
            Db.Delete(id);
        }
    }
}
