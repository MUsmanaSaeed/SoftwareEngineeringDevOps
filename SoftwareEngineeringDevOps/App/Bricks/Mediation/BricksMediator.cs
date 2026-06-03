using SoftwareEngineeringDevOps.App.Bricks.Repository;

namespace SoftwareEngineeringDevOps.App.Bricks
{
    public class BricksMediator : IBricksMediator
    {
        IBricksRepository Repository { get; }

        public BricksMediator(IBricksRepository repository)
        {
            Repository = repository;
        }

        public async Task<IEnumerable<IBrick>> GetAllBricks()
        {
            await Task.CompletedTask;
            return Repository.ListAll();
        }

        public async Task<IBrick?> GetBrickById(long id)
        {
            await Task.CompletedTask;
            return Repository.GetById(id);
        }

        public async Task<IEnumerable<IBrick>> GetBricksByManufacturerId(long manufacturerId)
        {
            await Task.CompletedTask;
            return Repository.GetByManufacturerId(manufacturerId);
        }

        public async Task<IBrick> Insert(NewBrick brick)
        {
            await Task.CompletedTask;
            return Repository.Insert(brick);
        }

        public async Task<IBrick> Update(EditBrick brick)
        {
            await Task.CompletedTask;
            return Repository.Update(brick);
        }

        public async Task Delete(long id)
        {
            await Task.CompletedTask;
            Repository.Delete(id);
        }
    }
}
