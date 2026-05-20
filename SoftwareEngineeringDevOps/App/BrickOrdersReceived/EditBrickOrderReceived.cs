namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived
{
    public class EditBrickOrderReceived
    {
        public EditBrickOrderReceived(IBrickOrderReceived brickOrderReceived)
        {
            BrickOrderReceived = brickOrderReceived;
            ResetValues();
        }

        IBrickOrderReceived BrickOrderReceived { get; }

        public long Id => BrickOrderReceived.Id;
        public int BricksReceived { get; set; }
        public DateTime ReceivedDate { get; set; }

        public void ResetValues()
        {
            BricksReceived = BrickOrderReceived.BricksReceived;
            ReceivedDate = BrickOrderReceived.ReceivedDate;
        }
    }
}
