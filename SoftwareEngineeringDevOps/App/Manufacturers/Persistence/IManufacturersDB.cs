using SoftwareEngineeringDevOps.App.Manufacturers.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.Manufacturers.Persistence
{
    public interface IManufacturersDB
    {
        IEnumerable<ManufacturerDBO> ListAll();
        ManufacturerDBO? GetById(long id);
        ManufacturerDBO Insert(NewManufacturer manufacturer);
        ManufacturerDBO Update(EditManufacturer manufacturer);
        void Delete(long id);
    }
}
