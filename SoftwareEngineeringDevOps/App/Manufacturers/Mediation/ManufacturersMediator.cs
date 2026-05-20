using SoftwareEngineeringDevOps.App.Manufacturers.Persistence;

namespace SoftwareEngineeringDevOps.App.Manufacturers
{
    public class ManufacturersMediator : IManufacturersMediator
    {
        IManufacturersDB Db { get; }

        public ManufacturersMediator(IManufacturersDB db)
        {
            Db = db;
        }

        public async Task<IEnumerable<IManufacturer>> GetAllManufacturers()
        {
            await Task.CompletedTask;
            return Db.ListAll().Select(Manufacturer.FromDBO);
        }

        public async Task<IManufacturer?> GetManufacturerById(long id)
        {
            await Task.CompletedTask;
            var dbo = Db.GetById(id);
            return dbo == null ? null : Manufacturer.FromDBO(dbo);
        }

        public async Task<IManufacturer> Insert(NewManufacturer manufacturer)
        {
            await Task.CompletedTask;
            var dbo = Db.Insert(manufacturer);
            return Manufacturer.FromDBO(dbo);
        }

        public async Task<IManufacturer> Update(EditManufacturer manufacturer)
        {
            await Task.CompletedTask;
            var dbo = Db.Update(manufacturer);
            return Manufacturer.FromDBO(dbo);
        }

        public async Task Delete(long id)
        {
            await Task.CompletedTask;
            Db.Delete(id);
        }
    }
}
