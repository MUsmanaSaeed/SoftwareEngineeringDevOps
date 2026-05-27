namespace SoftwareEngineeringDevOps.App.Manufacturers
{
    public class EditManufacturer
    {
        public EditManufacturer(IManufacturer manufacturer)
        {
            Manufacturer = manufacturer;
            ResetValues();
        }

        IManufacturer Manufacturer { get; }

        public long Id => Manufacturer.Id;
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string? Address2 { get; set; }
        public string Postcode { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }

        public void ResetValues()
        {
            Name = Manufacturer.Name;
            Address1 = Manufacturer.Address1;
            Address2 = Manufacturer.Address2;
            Postcode = Manufacturer.Postcode;
            PhoneNo = Manufacturer.PhoneNo;
            Email = Manufacturer.Email;
        }
    }
}
