using SoftwareEngineeringDevOps.App.BrickOrdersReceived.Persistence.DBOs;
using SoftwareEngineeringDevOps.App.Database;

namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived.Persistence
{
    public class BrickOrdersReceivedDB : DB, IBrickOrdersReceivedDB
    {
        public IEnumerable<BrickOrderReceivedDBO> ListAll()
        {
            return Select<BrickOrderReceivedDBO>("brickordersreceived_listall");
        }

        public BrickOrderReceivedDBO? GetById(long id)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", id },
            };
            return Select<BrickOrderReceivedDBO>("brickordersreceived_getbyid", parameters)?.FirstOrDefault();
        }

        public BrickOrderReceivedDBO Insert(NewBrickOrderReceived brickOrderReceived)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "BrickOrderId", brickOrderReceived.BrickOrderId },
                { "BricksReceived", brickOrderReceived.BricksReceived },
                { "ReceivedDate", brickOrderReceived.ReceivedDate.ToUniversalTime() },
            };
            return Select<BrickOrderReceivedDBO>("brickordersreceived_insert", parameters)?.FirstOrDefault();
        }

        public BrickOrderReceivedDBO Update(EditBrickOrderReceived brickOrderReceived)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", brickOrderReceived.Id },
                { "BricksReceived", brickOrderReceived.BricksReceived },
                { "ReceivedDate", brickOrderReceived.ReceivedDate.ToUniversalTime() },
            };
            return Select<BrickOrderReceivedDBO>("brickordersreceived_update", parameters)?.FirstOrDefault();
        }

        public void Delete(long id)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", id },
            };
            ExecuteWithParameters("brickordersreceived_delete", parameters);
        }
    }
}
