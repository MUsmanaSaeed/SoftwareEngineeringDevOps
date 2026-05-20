namespace SoftwareEngineeringDevOps.App.Bricks.Persistence.DBOs
{
    public class BrickDBO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long ManufacturerId { get; set; }
        public decimal Price { get; set; }
        public string Colour { get; set; }
        public string Material { get; set; }
        public decimal Strength { get; set; }
        public decimal Width { get; set; }
        public decimal Height { get; set; }
        public decimal Depth { get; set; }
        public string Type { get; set; }
        public decimal Voids { get; set; }
    }
}
