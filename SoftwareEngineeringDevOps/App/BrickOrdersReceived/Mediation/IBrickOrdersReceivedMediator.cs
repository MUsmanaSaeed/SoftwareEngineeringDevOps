namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived
{
    public interface IBrickOrdersReceivedMediator
    {
        Task<IEnumerable<IBrickOrderReceived>> GetAllBrickOrdersReceived();
        Task<IBrickOrderReceived?> GetBrickOrderReceivedById(long id);
        Task<IBrickOrderReceived> Insert(NewBrickOrderReceived brickOrderReceived);
        Task<IBrickOrderReceived> Update(EditBrickOrderReceived brickOrderReceived);
        Task Delete(long id);
    }
}
