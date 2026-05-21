namespace SoftwareEngineeringDevOps.App.BrickOrders.Persistence.DBOs
{
    public class BrickOrderDBO
    {
        public long Id { get; set; }
        public string OrderNo { get; set; }
        public long BrickId { get; set; }
        public int BricksOrdered { get; set; }
        public DateTime OrderedDate { get; set; }
        public DateTime ExpectedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public long CreatedById { get; set; }
    }
}
