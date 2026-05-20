using SoftwareEngineeringDevOps.App.Manufacturers.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.Manufacturers
{
    public class Manufacturer : IManufacturer
    {
        public static Manufacturer FromDBO(ManufacturerDBO dbo)
        {
            return new Manufacturer
            {
                Id = dbo.Id,
                Name = dbo.Name,
                Address1 = dbo.Address1,
                Address2 = dbo.Address2,
                Postcode = dbo.Postcode,
                PhoneNo = dbo.PhoneNo,
                Email = dbo.Email,
            };
        }

        public long Id { get; private set; }
        public string Name { get; private set; }
        public string Address1 { get; private set; }
        public string? Address2 { get; private set; }
        public string Postcode { get; private set; }
        public string PhoneNo { get; private set; }
        public string Email { get; private set; }
    }
}
