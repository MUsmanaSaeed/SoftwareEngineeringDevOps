using SoftwareEngineeringDevOps.App.BrickOrders.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.BrickOrders.Persistence
{
    public interface IBrickOrdersDB
    {
        IEnumerable<BrickOrderDBO> ListAll();
        BrickOrderDBO? GetById(long id);
        BrickOrderDBO Insert(NewBrickOrder brickOrder);
        BrickOrderDBO Update(EditBrickOrder brickOrder);
        BrickOrderDBO Cancel(long id, DateTime cancelledDate);
        BrickOrderDBO Uncancel(long id);
        void Delete(long id);
    }
}
