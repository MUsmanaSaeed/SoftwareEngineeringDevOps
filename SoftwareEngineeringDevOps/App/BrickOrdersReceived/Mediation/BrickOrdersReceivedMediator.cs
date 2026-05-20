using SoftwareEngineeringDevOps.App.BrickOrdersReceived.Persistence;

namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived
{
    public class BrickOrdersReceivedMediator : IBrickOrdersReceivedMediator
    {
        IBrickOrdersReceivedDB Db { get; }

        public BrickOrdersReceivedMediator(IBrickOrdersReceivedDB db)
        {
            Db = db;
        }

        public async Task<IEnumerable<IBrickOrderReceived>> GetAllBrickOrdersReceived()
        {
            await Task.CompletedTask;
            return Db.ListAll().Select(BrickOrderReceived.FromDBO);
        }

        public async Task<IBrickOrderReceived?> GetBrickOrderReceivedById(long id)
        {
            await Task.CompletedTask;
            var dbo = Db.GetById(id);
            return dbo == null ? null : BrickOrderReceived.FromDBO(dbo);
        }

        public async Task<IBrickOrderReceived> Insert(NewBrickOrderReceived brickOrderReceived)
        {
            await Task.CompletedTask;
            var dbo = Db.Insert(brickOrderReceived);
            return BrickOrderReceived.FromDBO(dbo);
        }

        public async Task<IBrickOrderReceived> Update(EditBrickOrderReceived brickOrderReceived)
        {
            await Task.CompletedTask;
            var dbo = Db.Update(brickOrderReceived);
            return BrickOrderReceived.FromDBO(dbo);
        }

        public async Task Delete(long id)
        {
            await Task.CompletedTask;
            Db.Delete(id);
        }
    }
}
