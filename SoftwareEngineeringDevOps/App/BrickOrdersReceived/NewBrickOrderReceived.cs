namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived
{
    public class NewBrickOrderReceived
    {
        public long BrickOrderId { get; set; }
        public int BricksReceived { get; set; }
        public DateTime ReceivedDate { get; set; }
        public long ReceivedById { get; set; }
    }
}
