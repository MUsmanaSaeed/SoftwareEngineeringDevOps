namespace SoftwareEngineeringDevOps.App.Manufacturers.Repository
{
    public interface IManufacturersRepository
    {
        IEnumerable<IManufacturer> ListAll();
        IManufacturer? GetById(long id);
        IManufacturer Insert(NewManufacturer manufacturer);
        IManufacturer Update(EditManufacturer manufacturer);
        void Delete(long id);
    }
}
