namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived.Repository
{
    public interface IBrickOrdersReceivedRepository
    {
        IEnumerable<IBrickOrderReceived> ListAll();
        IBrickOrderReceived? GetById(long id);
        IEnumerable<IBrickOrderReceived> GetByBrickOrderId(long brickOrderId);
        IBrickOrderReceived Insert(NewBrickOrderReceived brickOrderReceived);
        IBrickOrderReceived Update(EditBrickOrderReceived brickOrderReceived);
        void Delete(long id);
    }
}
