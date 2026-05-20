using SoftwareEngineeringDevOps.App.BrickOrders.Persistence;

namespace SoftwareEngineeringDevOps.App.BrickOrders
{
    public class BrickOrdersMediator : IBrickOrdersMediator
    {
        IBrickOrdersDB Db { get; }

        public BrickOrdersMediator(IBrickOrdersDB db)
        {
            Db = db;
        }

        public async Task<IEnumerable<IBrickOrder>> GetAllBrickOrders()
        {
            await Task.CompletedTask;
            return Db.ListAll().Select(BrickOrder.FromDBO);
        }

        public async Task<IBrickOrder?> GetBrickOrderById(long id)
        {
            await Task.CompletedTask;
            var dbo = Db.GetById(id);
            return dbo == null ? null : BrickOrder.FromDBO(dbo);
        }

        public async Task<IBrickOrder> Insert(NewBrickOrder brickOrder)
        {
            await Task.CompletedTask;
            var dbo = Db.Insert(brickOrder);
            return BrickOrder.FromDBO(dbo);
        }

        public async Task<IBrickOrder> Update(EditBrickOrder brickOrder)
        {
            await Task.CompletedTask;
            var dbo = Db.Update(brickOrder);
            return BrickOrder.FromDBO(dbo);
        }

        public async Task<IBrickOrder> Cancel(long id)
        {
            await Task.CompletedTask;
            var dbo = Db.Cancel(id, DateTime.UtcNow);
            return BrickOrder.FromDBO(dbo);
        }

        public async Task<IBrickOrder> Uncancel(long id)
        {
            await Task.CompletedTask;
            var dbo = Db.Uncancel(id);
            return BrickOrder.FromDBO(dbo);
        }

        public async Task Delete(long id)
        {
            await Task.CompletedTask;
            Db.Delete(id);
        }
    }
}
