namespace SoftwareEngineeringDevOps.App.BrickOrders
{
    public class EditBrickOrder
    {
        public EditBrickOrder(IBrickOrder brickOrder)
        {
            BrickOrder = brickOrder;
            ResetValues();
        }

        IBrickOrder BrickOrder { get; }

        public long Id => BrickOrder.Id;
        public string OrderNo => BrickOrder.OrderNo;
        public long BrickId { get; set; }
        public int BricksOrdered { get; set; }
        public DateTime OrderedDate { get; set; }
        public DateTime ExpectedDate { get; set; }

        public void ResetValues()
        {
            BrickId = BrickOrder.BrickId;
            BricksOrdered = BrickOrder.BricksOrdered;
            OrderedDate = BrickOrder.OrderedDate;
            ExpectedDate = BrickOrder.ExpectedDate;
        }
    }
}
