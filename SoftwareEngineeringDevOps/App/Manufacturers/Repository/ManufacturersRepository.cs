using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.Manufacturers.Persistence;
using SoftwareEngineeringDevOps.App.Manufacturers.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.Manufacturers.Repository
{
    /// <summary>
    /// Sits between the mediator and the DB layer.
    /// Caches manufacturers by id so repeated lookups never hit the database more than once per id.
    /// </summary>
    public class ManufacturersRepository : IManufacturersRepository
    {
        private readonly IManufacturersDB _db;
        private readonly ILogger<ManufacturersRepository> _logger;
        private readonly Dictionary<long, IManufacturer> _cacheById = [];

        public ManufacturersRepository(IManufacturersDB db, ILogger<ManufacturersRepository> logger)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(logger);
            _db = db;
            _logger = logger;
        }

        public IEnumerable<IManufacturer> ListAll()
        {
            try
            {
                IEnumerable<Manufacturer> manufacturers = _db.ListAll().Select(Manufacturer.FromDBO).ToList();

                foreach (Manufacturer manufacturer in manufacturers)
                {
                    _cacheById[manufacturer.Id] = manufacturer;
                }

                return manufacturers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to list all manufacturers from repository");
                throw;
            }
        }

        public IManufacturer? GetById(long id)
        {
            try
            {
                if (_cacheById.TryGetValue(id, out var cached))
                {
                    return cached;
                }

                ManufacturerDBO? dbo = _db.GetById(id);

                if (dbo == null)
                {
                    _logger.LogWarning("Manufacturer not found: {ManufacturerId}", id);
                    return null;
                }

                Manufacturer manufacturer = Manufacturer.FromDBO(dbo);
                _cacheById[manufacturer.Id] = manufacturer;
                return manufacturer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get manufacturer by ID: {ManufacturerId}", id);
                throw;
            }
        }

        public IManufacturer Insert(NewManufacturer manufacturer)
        {
            ArgumentNullException.ThrowIfNull(manufacturer);

            try
            {
                ManufacturerDBO dbo = _db.Insert(manufacturer);
                Manufacturer created = Manufacturer.FromDBO(dbo);
                _cacheById[created.Id] = created;
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert manufacturer: {ManufacturerName}", manufacturer.Name);
                throw;
            }
        }

        public IManufacturer Update(EditManufacturer manufacturer)
        {
            ArgumentNullException.ThrowIfNull(manufacturer);

            try
            {
                ManufacturerDBO dbo = _db.Update(manufacturer);
                Manufacturer updated = Manufacturer.FromDBO(dbo);
                _cacheById[updated.Id] = updated;
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update manufacturer: {ManufacturerId} - {ManufacturerName}", 
                    manufacturer.Id, manufacturer.Name);
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
                _logger.LogError(ex, "Failed to delete manufacturer: {ManufacturerId}", id);
                throw;
            }
        }
    }
}
