using SoftwareEngineeringDevOps.App.Bricks.Persistence;
using SoftwareEngineeringDevOps.App.Manufacturers;

namespace SoftwareEngineeringDevOps.App.Bricks
{
    public class BricksMediator : IBricksMediator
    {
        IBricksDB Db { get; }
        IManufacturersMediator ManufacturersMediator { get; }

        public BricksMediator(IBricksDB db, IManufacturersMediator manufacturersMediator)
        {
            Db = db;
            ManufacturersMediator = manufacturersMediator;
        }

        private IManufacturerInfo GetManufacturer(long id)
        {
            return ManufacturersMediator.GetManufacturerById(id).GetAwaiter().GetResult()
                ?? throw new InvalidOperationException($"Manufacturer {id} not found.");
        }

        public async Task<IEnumerable<IBrick>> GetAllBricks()
        {
            await Task.CompletedTask;
            return Db.ListAll().Select(dbo => Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId)));
        }

        public async Task<IBrick?> GetBrickById(long id)
        {
            await Task.CompletedTask;
            var dbo = Db.GetById(id);
            return dbo == null ? null : Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId));
        }

        public async Task<IEnumerable<IBrick>> GetBricksByManufacturerId(long manufacturerId)
        {
            await Task.CompletedTask;
            return Db.GetByManufacturerId(manufacturerId).Select(dbo => Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId)));
        }

        public async Task<IBrick> Insert(NewBrick brick)
        {
            await Task.CompletedTask;
            var dbo = Db.Insert(brick);
            return Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId));
        }

        public async Task<IBrick> Update(EditBrick brick)
        {
            await Task.CompletedTask;
            var dbo = Db.Update(brick);
            return Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId));
        }

        public async Task Delete(long id)
        {
            await Task.CompletedTask;
            Db.Delete(id);
        }
    }
}
