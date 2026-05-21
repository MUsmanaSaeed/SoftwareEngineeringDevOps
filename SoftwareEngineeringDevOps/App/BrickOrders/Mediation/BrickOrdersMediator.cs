using SoftwareEngineeringDevOps.App.BrickOrders.Persistence;
using SoftwareEngineeringDevOps.App.Users;

namespace SoftwareEngineeringDevOps.App.BrickOrders
{
    public class BrickOrdersMediator : IBrickOrdersMediator
    {
        IBrickOrdersDB Db { get; }
        IUsersMediator UsersMediator { get; }

        public BrickOrdersMediator(IBrickOrdersDB db, IUsersMediator usersMediator)
        {
            Db = db;
            UsersMediator = usersMediator;
        }

        IUserInfo GetUser(long id)
        {
            return UsersMediator.GetUserById(id).GetAwaiter().GetResult()
                ?? throw new InvalidOperationException($"User {id} not found.");
        }

        public async Task<IEnumerable<IBrickOrder>> GetAllBrickOrders()
        {
            await Task.CompletedTask;
            return Db.ListAll().Select(dbo => BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById)));
        }

        public async Task<IBrickOrder?> GetBrickOrderById(long id)
        {
            await Task.CompletedTask;
            var dbo = Db.GetById(id);
            return dbo == null ? null : BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
        }

        public async Task<IBrickOrder> Insert(NewBrickOrder brickOrder)
        {
            await Task.CompletedTask;
            var dbo = Db.Insert(brickOrder);
            return BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
        }

        public async Task<IBrickOrder> Update(EditBrickOrder brickOrder)
        {
            await Task.CompletedTask;
            var dbo = Db.Update(brickOrder);
            return BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
        }

        public async Task<IBrickOrder> Cancel(long id)
        {
            await Task.CompletedTask;
            var dbo = Db.Cancel(id, DateTime.UtcNow);
            return BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
        }

        public async Task<IBrickOrder> Uncancel(long id)
        {
            await Task.CompletedTask;
            var dbo = Db.Uncancel(id);
            return BrickOrder.FromDBO(dbo, GetUser(dbo.CreatedById));
        }

        public async Task Delete(long id)
        {
            await Task.CompletedTask;
            Db.Delete(id);
        }
    }
}
