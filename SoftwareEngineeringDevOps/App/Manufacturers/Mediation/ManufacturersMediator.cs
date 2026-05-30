using SoftwareEngineeringDevOps.App.Manufacturers.Repository;

namespace SoftwareEngineeringDevOps.App.Manufacturers
{
    public class ManufacturersMediator : IManufacturersMediator
    {
        IManufacturersRepository Repository { get; }

        public ManufacturersMediator(IManufacturersRepository repository)
        {
            Repository = repository;
        }

        public async Task<IEnumerable<IManufacturer>> GetAllManufacturers()
        {
            await Task.CompletedTask;
            return Repository.ListAll();
        }

        public async Task<IManufacturer?> GetManufacturerById(long id)
        {
            await Task.CompletedTask;
            return Repository.GetById(id);
        }

        public async Task<IManufacturer> Insert(NewManufacturer manufacturer)
        {
            await Task.CompletedTask;
            return Repository.Insert(manufacturer);
        }

        public async Task<IManufacturer> Update(EditManufacturer manufacturer)
        {
            await Task.CompletedTask;
            return Repository.Update(manufacturer);
        }

        public async Task Delete(long id)
        {
            await Task.CompletedTask;
            Repository.Delete(id);
        }
    }
}
