namespace SoftwareEngineeringDevOps.App.Manufacturers
{
    public interface IManufacturer : IManufacturerInfo
    {
        string Address1 { get; }
        string? Address2 { get; }
        string Postcode { get; }
        string PhoneNo { get; }
        string Email { get; }
    }
}
