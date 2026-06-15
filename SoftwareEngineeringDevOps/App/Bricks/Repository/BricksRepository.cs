using Microsoft.Extensions.Logging;
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
        private readonly IBricksDB _db;
        private readonly IManufacturersRepository _manufacturersRepository;
        private readonly ILogger<BricksRepository> _logger;
        private readonly Dictionary<long, IBrick> _cacheById = [];

        public BricksRepository(IBricksDB db, IManufacturersRepository manufacturersRepository, ILogger<BricksRepository> logger)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(manufacturersRepository);
            ArgumentNullException.ThrowIfNull(logger);
            _db = db;
            _manufacturersRepository = manufacturersRepository;
            _logger = logger;
        }

        private IManufacturerInfo GetManufacturer(long id)
        {
            try
            {
                var manufacturer = _manufacturersRepository.GetById(id);

                if (manufacturer == null)
                {
                    _logger.LogError("Manufacturer not found: {ManufacturerId}", id);
                    throw new InvalidOperationException($"Manufacturer {id} not found.");
                }

                return manufacturer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to resolve manufacturer: {ManufacturerId}", id);
                throw;
            }
        }

        public IEnumerable<IBrick> ListAll()
        {
            try
            {
                IEnumerable<Brick> bricks = _db.ListAll()
                    .Select(dbo => Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId)))
                    .ToList();

                foreach (Brick brick in bricks)
                {
                    _cacheById[brick.Id] = brick;
                }

                return bricks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list all bricks from repository");
                throw;
            }
        }

        public IBrick? GetById(long id)
        {
            try
            {
                if (_cacheById.TryGetValue(id, out var cached))
                {
                    return cached;
                }

                BrickDBO? dbo = _db.GetById(id);

                if (dbo == null)
                {
                    _logger.LogWarning("Brick not found: {BrickId}", id);
                    return null;
                }

                Brick brick = Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId));
                _cacheById[brick.Id] = brick;
                return brick;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get brick by ID: {BrickId}", id);
                throw;
            }
        }

        public IEnumerable<IBrick> GetByManufacturerId(long manufacturerId)
        {
            try
            {
                IEnumerable<Brick> bricks = _db.GetByManufacturerId(manufacturerId)
                    .Select(dbo => Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId)))
                    .ToList();

                foreach (Brick brick in bricks)
                {
                    _cacheById[brick.Id] = brick;
                }

                return bricks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get bricks by manufacturer ID: {ManufacturerId}", manufacturerId);
                throw;
            }
        }

        public IBrick Insert(NewBrick brick)
        {
            ArgumentNullException.ThrowIfNull(brick);

            try
            {
                BrickDBO dbo = _db.Insert(brick);
                Brick created = Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId));
                _cacheById[created.Id] = created;
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert brick: {BrickName}", brick.Name);
                throw;
            }
        }

        public IBrick Update(EditBrick brick)
        {
            ArgumentNullException.ThrowIfNull(brick);

            try
            {
                BrickDBO dbo = _db.Update(brick);

                // Invalidate cache so the updated brick is re-resolved with fresh manufacturer info
                _cacheById.Remove(dbo.Id);

                Brick updated = Brick.FromDBO(dbo, GetManufacturer(dbo.ManufacturerId));
                _cacheById[updated.Id] = updated;
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update brick: {BrickId} - {BrickName}", brick.Id, brick.Name);
                throw;
            }
        }

        public void Delete(long id)
        {
            try
            {
                _db.Delete(id);
                _cacheById.Remove(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete brick: {BrickId}", id);
                throw;
            }
        }
    }
}
