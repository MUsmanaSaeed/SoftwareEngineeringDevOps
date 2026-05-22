using SoftwareEngineeringDevOps.App.Bricks.Persistence.DBOs;
using SoftwareEngineeringDevOps.App.Manufacturers;

namespace SoftwareEngineeringDevOps.App.Bricks
{
    public class Brick : IBrick
    {
        public static Brick FromDBO(BrickDBO dbo, IManufacturerInfo manufacturer)
        {
            return new Brick
            {
                Id = dbo.Id,
                Name = dbo.Name,
                Manufacturer = manufacturer,
                Price = dbo.Price,
                Colour = dbo.Colour,
                Material = dbo.Material,
                Strength = dbo.Strength,
                Width = dbo.Width,
                Height = dbo.Height,
                Depth = dbo.Depth,
                Type = dbo.Type,
                Voids = dbo.Voids,
            };
        }

        public long Id { get; private set; }
        public string Name { get; private set; }
        public IManufacturerInfo Manufacturer { get; private set; }
        public decimal Price { get; private set; }
        public string Colour { get; private set; }
        public string Material { get; private set; }
        public decimal Strength { get; private set; }
        public decimal Width { get; private set; }
        public decimal Height { get; private set; }
        public decimal Depth { get; private set; }
        public string Type { get; private set; }
        public decimal Voids { get; private set; }
    }
}
