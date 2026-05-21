namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived.Persistence.DBOs
{
    public class BrickOrderReceivedDBO
    {
        public long Id { get; set; }
        public long BrickOrderId { get; set; }
        public int BricksReceived { get; set; }
        public DateTime ReceivedDate { get; set; }
        public long ReceivedById { get; set; }
    }
}
