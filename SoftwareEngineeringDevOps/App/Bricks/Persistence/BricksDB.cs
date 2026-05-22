using SoftwareEngineeringDevOps.App.Bricks.Persistence.DBOs;
using SoftwareEngineeringDevOps.App.Database;

namespace SoftwareEngineeringDevOps.App.Bricks.Persistence
{
    public class BricksDB : DB, IBricksDB
    {
        public IEnumerable<BrickDBO> ListAll()
        {
            return Select<BrickDBO>("bricks_listall");
        }

        public BrickDBO? GetById(long id)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", id },
            };
            return Select<BrickDBO>("bricks_getbyid", parameters)?.FirstOrDefault();
        }

        public IEnumerable<BrickDBO> GetByManufacturerId(long manufacturerId)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "ManufacturerId", manufacturerId },
            };
            return Select<BrickDBO>("bricks_getbymanufacturerid", parameters) ?? [];
        }

        public BrickDBO Insert(NewBrick brick)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Name", brick.Name },
                { "ManufacturerId", brick.ManufacturerId },
                { "Price", brick.Price },
                { "Colour", brick.Colour },
                { "Material", brick.Material },
                { "Strength", brick.Strength },
                { "Width", brick.Width },
                { "Height", brick.Height },
                { "Depth", brick.Depth },
                { "Type", brick.Type },
                { "Voids", brick.Voids },
            };
            return Select<BrickDBO>("bricks_insert", parameters)?.FirstOrDefault();
        }

        public BrickDBO Update(EditBrick brick)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", brick.Id },
                { "Name", brick.Name },
                { "ManufacturerId", brick.ManufacturerId },
                { "Price", brick.Price },
                { "Colour", brick.Colour },
                { "Material", brick.Material },
                { "Strength", brick.Strength },
                { "Width", brick.Width },
                { "Height", brick.Height },
                { "Depth", brick.Depth },
                { "Type", brick.Type },
                { "Voids", brick.Voids },
            };
            return Select<BrickDBO>("bricks_update", parameters)?.FirstOrDefault();
        }

        public void Delete(long id)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", id },
            };
            ExecuteWithParameters("bricks_delete", parameters);
        }
    }
}
