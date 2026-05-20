namespace SoftwareEngineeringDevOps.App.Bricks
{
    public class NewBrick
    {
        public string Name { get; set; }
        public long ManufacturerId { get; set; }
        public decimal Price { get; set; } = 0;
        public string Colour { get; set; }
        public string Material { get; set; }
        public decimal Strength { get; set; } = 0;
        public decimal Width { get; set; } = 0;
        public decimal Height { get; set; } = 0;
        public decimal Depth { get; set; } = 0;
        public string Type { get; set; }
        public decimal Voids { get; set; } = 0;
    }
}
