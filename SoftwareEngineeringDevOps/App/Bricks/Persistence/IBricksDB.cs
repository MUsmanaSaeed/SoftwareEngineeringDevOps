using SoftwareEngineeringDevOps.App.Bricks.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.Bricks.Persistence
{
    public interface IBricksDB
    {
        IEnumerable<BrickDBO> ListAll();
        BrickDBO? GetById(long id);
        IEnumerable<BrickDBO> GetByManufacturerId(long manufacturerId);
        BrickDBO Insert(NewBrick brick);
        BrickDBO Update(EditBrick brick);
        void Delete(long id);
    }
}
