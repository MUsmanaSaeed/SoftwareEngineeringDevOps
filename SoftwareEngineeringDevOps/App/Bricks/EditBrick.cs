namespace SoftwareEngineeringDevOps.App.Bricks
{
    public class EditBrick
    {
        public EditBrick(IBrick brick)
        {
            Brick = brick;
            ResetValues();
        }

        IBrick Brick { get; }

        public long Id => Brick.Id;
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

        public void ResetValues()
        {
            Name = Brick.Name;
            ManufacturerId = Brick.Manufacturer.Id;
            Price = Brick.Price;
            Colour = Brick.Colour;
            Material = Brick.Material;
            Strength = Brick.Strength;
            Width = Brick.Width;
            Height = Brick.Height;
            Depth = Brick.Depth;
            Type = Brick.Type;
            Voids = Brick.Voids;
        }
    }
}
