namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived
{
    public interface IBrickOrderReceived
    {
        long Id { get; }
        long BrickOrderId { get; }
        int BricksReceived { get; }
        DateTime ReceivedDate { get; }
    }
}
