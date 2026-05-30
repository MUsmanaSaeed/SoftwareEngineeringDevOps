namespace SoftwareEngineeringDevOps.App.BrickOrders.Repository
{
    public interface IBrickOrdersRepository
    {
        IEnumerable<IBrickOrder> ListAll();
        IBrickOrder? GetById(long id);
        IEnumerable<IBrickOrder> GetByBrickId(long brickId);
        IBrickOrder Insert(NewBrickOrder brickOrder);
        IBrickOrder Update(EditBrickOrder brickOrder);
        IBrickOrder Cancel(long id, DateTime cancelledDate);
        IBrickOrder Uncancel(long id);
        void Delete(long id);
    }
}
