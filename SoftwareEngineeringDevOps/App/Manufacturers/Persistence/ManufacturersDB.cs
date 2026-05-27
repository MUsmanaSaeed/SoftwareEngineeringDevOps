using SoftwareEngineeringDevOps.App.Database;
using SoftwareEngineeringDevOps.App.Manufacturers.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.Manufacturers.Persistence
{
    public class ManufacturersDB : DB, IManufacturersDB
    {
        public IEnumerable<ManufacturerDBO> ListAll()
        {
            return Select<ManufacturerDBO>("manufacturers_listall");
        }

        public ManufacturerDBO? GetById(long id)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", id },
            };
            return Select<ManufacturerDBO>("manufacturers_getbyid", parameters)?.FirstOrDefault();
        }

        public ManufacturerDBO Insert(NewManufacturer manufacturer)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Name", manufacturer.Name },
                { "Address1", manufacturer.Address1 },
                { "Address2", manufacturer.Address2 },
                { "Postcode", manufacturer.Postcode },
                { "PhoneNo", manufacturer.PhoneNo },
                { "Email", manufacturer.Email },
            };
            return Select<ManufacturerDBO>("manufacturers_insert", parameters)?.FirstOrDefault();
        }

        public ManufacturerDBO Update(EditManufacturer manufacturer)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", manufacturer.Id },
                { "Name", manufacturer.Name },
                { "Address1", manufacturer.Address1 },
                { "Address2", manufacturer.Address2 },
                { "Postcode", manufacturer.Postcode },
                { "PhoneNo", manufacturer.PhoneNo },
                { "Email", manufacturer.Email },
            };
            return Select<ManufacturerDBO>("manufacturers_update", parameters)?.FirstOrDefault();
        }

        public void Delete(long id)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", id },
            };
            ExecuteWithParameters("manufacturers_delete", parameters);
        }
    }
}
