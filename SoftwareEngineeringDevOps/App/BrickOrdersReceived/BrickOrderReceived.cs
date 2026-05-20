using SoftwareEngineeringDevOps.App.BrickOrdersReceived.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived
{
    public class BrickOrderReceived : IBrickOrderReceived
    {
        public static BrickOrderReceived FromDBO(BrickOrderReceivedDBO dbo)
        {
            return new BrickOrderReceived
            {
                Id = dbo.Id,
                BrickOrderId = dbo.BrickOrderId,
                BricksReceived = dbo.BricksReceived,
                ReceivedDate = dbo.ReceivedDate.ToUkTime(),
            };
        }

        public long Id { get; private set; }
        public long BrickOrderId { get; private set; }
        public int BricksReceived { get; private set; }
        public DateTime ReceivedDate { get; private set; }
    }
}
