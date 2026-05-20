namespace SoftwareEngineeringDevOps.App.Bricks
{
    public interface IBrick
    {
        long Id { get; }
        string Name { get; }
        long ManufacturerId { get; }
        decimal Price { get; }
        string Colour { get; }
        string Material { get; }
        decimal Strength { get; }
        decimal Width { get; }
        decimal Height { get; }
        decimal Depth { get; }
        string Type { get; }
        decimal Voids { get; }
    }
}
