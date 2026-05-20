namespace SoftwareEngineeringDevOps.App.BrickOrders
{
    public interface IBrickOrdersMediator
    {
        Task<IEnumerable<IBrickOrder>> GetAllBrickOrders();
        Task<IBrickOrder?> GetBrickOrderById(long id);
        Task<IBrickOrder> Insert(NewBrickOrder brickOrder);
        Task<IBrickOrder> Update(EditBrickOrder brickOrder);
        Task<IBrickOrder> Uncancel(long id);
        Task Delete(long id);
        Task<IBrickOrder> Cancel(long id);
    }
}
