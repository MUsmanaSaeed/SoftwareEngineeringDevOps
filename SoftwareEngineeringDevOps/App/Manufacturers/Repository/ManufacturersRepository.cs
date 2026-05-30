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
        IManufacturersDB Db { get; }
        readonly Dictionary<long, IManufacturer> _cacheById = [];

        public ManufacturersRepository(IManufacturersDB db)
        {
            Db = db;
        }

        public IEnumerable<IManufacturer> ListAll()
        {
            IEnumerable<Manufacturer> manufacturers = Db.ListAll().Select(Manufacturer.FromDBO).ToList();
            foreach (Manufacturer manufacturer in manufacturers)
            {
                _cacheById[manufacturer.Id] = manufacturer;
            }
            return manufacturers;
        }

        public IManufacturer? GetById(long id)
        {
            if (_cacheById.TryGetValue(id, out var cached)) return cached;
            ManufacturerDBO? dbo = Db.GetById(id);
            if (dbo == null) return null;
            Manufacturer manufacturer = Manufacturer.FromDBO(dbo);
            _cacheById[manufacturer.Id] = manufacturer;
            return manufacturer;
        }

        public IManufacturer Insert(NewManufacturer manufacturer)
        {
            ManufacturerDBO dbo = Db.Insert(manufacturer);
            Manufacturer created = Manufacturer.FromDBO(dbo);
            _cacheById[created.Id] = created;
            return created;
        }

        public IManufacturer Update(EditManufacturer manufacturer)
        {
            ManufacturerDBO dbo = Db.Update(manufacturer);
            Manufacturer updated = Manufacturer.FromDBO(dbo);
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
