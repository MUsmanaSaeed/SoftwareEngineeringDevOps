using SoftwareEngineeringDevOps.App.BrickOrders.Repository;

namespace SoftwareEngineeringDevOps.App.BrickOrders
{
    public class BrickOrdersMediator : IBrickOrdersMediator
    {
        IBrickOrdersRepository Repository { get; }

        public BrickOrdersMediator(IBrickOrdersRepository repository)
        {
            Repository = repository;
        }

        public async Task<IEnumerable<IBrickOrder>> GetAllBrickOrders()
        {
            await Task.CompletedTask;
            return Repository.ListAll();
        }

        public async Task<IBrickOrder?> GetBrickOrderById(long id)
        {
            await Task.CompletedTask;
            return Repository.GetById(id);
        }

        public async Task<IEnumerable<IBrickOrder>> GetBrickOrdersByBrickId(long brickId)
        {
            await Task.CompletedTask;
            return Repository.GetByBrickId(brickId);
        }

        public async Task<IBrickOrder> Insert(NewBrickOrder brickOrder)
        {
            await Task.CompletedTask;
            return Repository.Insert(brickOrder);
        }

        public async Task<IBrickOrder> Update(EditBrickOrder brickOrder)
        {
            await Task.CompletedTask;
            return Repository.Update(brickOrder);
        }

        public async Task<IBrickOrder> Cancel(long id)
        {
            await Task.CompletedTask;
            return Repository.Cancel(id, DateTime.UtcNow);
        }

        public async Task<IBrickOrder> Uncancel(long id)
        {
            await Task.CompletedTask;
            return Repository.Uncancel(id);
        }

        public async Task Delete(long id)
        {
            await Task.CompletedTask;
            Repository.Delete(id);
        }
    }
}
