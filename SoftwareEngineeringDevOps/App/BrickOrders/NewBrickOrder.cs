namespace SoftwareEngineeringDevOps.App.BrickOrders
{
    public class NewBrickOrder
    {
        public string OrderNo { get; set; }
        public long BrickId { get; set; }
        public int BricksOrdered { get; set; } = 0;
        public DateTime OrderedDate { get; set; }
        public DateTime ExpectedDate { get; set; }
    }
}
