namespace SoftwareEngineeringDevOps.App.Manufacturers
{
    public interface IManufacturersMediator
    {
        Task<IEnumerable<IManufacturer>> GetAllManufacturers();
        Task<IManufacturer?> GetManufacturerById(long id);
        Task<IManufacturer> Insert(NewManufacturer manufacturer);
        Task<IManufacturer> Update(EditManufacturer manufacturer);
        Task Delete(long id);
    }
}
