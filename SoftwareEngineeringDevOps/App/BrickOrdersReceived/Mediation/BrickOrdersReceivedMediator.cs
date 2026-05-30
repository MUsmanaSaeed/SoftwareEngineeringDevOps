using SoftwareEngineeringDevOps.App.BrickOrdersReceived.Repository;

namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived
{
    public class BrickOrdersReceivedMediator : IBrickOrdersReceivedMediator
    {
        IBrickOrdersReceivedRepository Repository { get; }

        public BrickOrdersReceivedMediator(IBrickOrdersReceivedRepository repository)
        {
            Repository = repository;
        }

        public async Task<IEnumerable<IBrickOrderReceived>> GetAllBrickOrdersReceived()
        {
            await Task.CompletedTask;
            return Repository.ListAll();
        }

        public async Task<IBrickOrderReceived?> GetBrickOrderReceivedById(long id)
        {
            await Task.CompletedTask;
            return Repository.GetById(id);
        }

        public async Task<IEnumerable<IBrickOrderReceived>> GetBrickOrdersReceivedByBrickOrderId(long brickOrderId)
        {
            await Task.CompletedTask;
            return Repository.GetByBrickOrderId(brickOrderId);
        }

        public async Task<IBrickOrderReceived> Insert(NewBrickOrderReceived brickOrderReceived)
        {
            await Task.CompletedTask;
            return Repository.Insert(brickOrderReceived);
        }

        public async Task<IBrickOrderReceived> Update(EditBrickOrderReceived brickOrderReceived)
        {
            await Task.CompletedTask;
            return Repository.Update(brickOrderReceived);
        }

        public async Task Delete(long id)
        {
            await Task.CompletedTask;
            Repository.Delete(id);
        }
    }
}
