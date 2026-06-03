using SoftwareEngineeringDevOps.App.Bricks.Persistence;
using SoftwareEngineeringDevOps.App.Bricks.Persistence.DBOs;
using SoftwareEngineeringDevOps.App.Manufacturers;
using SoftwareEngineeringDevOps.App.Manufacturers.Repository;

namespace SoftwareEngineeringDevOps.App.Bricks.Repository
{
    /// <summary>
    /// Sits between the mediator and the DB layer.
    /// Caches bricks by id and resolves manufacturer info via IManufacturersRepository.
    /// </summary>
    public class BricksRepository : IBricksRepository
    {
        IBricksDB Db { get; }
        IManufacturersRepository ManufacturersRepository { get; }
        readonly Dictionary<long, IBrick> _cacheById = [];

        public BricksRepository(IBricksDB db, IManufacturersRepository manufacturersRepository)
        {
            Db = db;
            ManufacturersRepository = manufacturersRepository;
        }

        private IManufacturerInfo GetManufacturer(long id)
        {
            return ManufacturersRepository.GetById(id)
                ?? throw new InvalidOperationException($"Manufacturer {id} not found.");
        }

        public IEnumerable<IBrick> ListAll()
        {
            IEnumerable<Brick> bricks = Db.ListAll()
                .Select(dbo => Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId)))
                .ToList();
            foreach (Brick brick in bricks)
            {
                _cacheById[brick.Id] = brick;
            }
            return bricks;
        }

        public IBrick? GetById(long id)
        {
            if (_cacheById.TryGetValue(id, out var cached)) return cached;
            BrickDBO? dbo = Db.GetById(id);
            if (dbo == null) return null;
            Brick brick = Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId));
            _cacheById[brick.Id] = brick;
            return brick;
        }

        public IEnumerable<IBrick> GetByManufacturerId(long manufacturerId)
        {
            IEnumerable<Brick> bricks = Db.GetByManufacturerId(manufacturerId)
                .Select(dbo => Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId)))
                .ToList();
            foreach (Brick brick in bricks)
            {
                _cacheById[brick.Id] = brick;
            }
            return bricks;
        }

        public IBrick Insert(NewBrick brick)
        {
            BrickDBO dbo = Db.Insert(brick);
            Brick created = Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId));
            _cacheById[created.Id] = created;
            return created;
        }

        public IBrick Update(EditBrick brick)
        {
            BrickDBO dbo = Db.Update(brick);
            // Invalidate cache so the updated brick is re-resolved with fresh manufacturer info
            _cacheById.Remove(dbo.Id);
            Brick updated = Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId));
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
