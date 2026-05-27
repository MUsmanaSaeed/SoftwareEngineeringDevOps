namespace SoftwareEngineeringDevOps.App.Manufacturers.Persistence.DBOs
{
    public class ManufacturerDBO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string? Address2 { get; set; }
        public string Postcode { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
    }
}
