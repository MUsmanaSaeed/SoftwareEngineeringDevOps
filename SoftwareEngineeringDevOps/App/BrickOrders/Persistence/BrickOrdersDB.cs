using SoftwareEngineeringDevOps.App.BrickOrders.Persistence.DBOs;
using SoftwareEngineeringDevOps.App.Database;

namespace SoftwareEngineeringDevOps.App.BrickOrders.Persistence
{
    public class BrickOrdersDB : DB, IBrickOrdersDB
    {
        public IEnumerable<BrickOrderDBO> ListAll()
        {
            return Select<BrickOrderDBO>("brickorders_listall");
        }

        public BrickOrderDBO? GetById(long id)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", id },
            };
            return Select<BrickOrderDBO>("brickorders_getbyid", parameters)?.FirstOrDefault();
        }

        public BrickOrderDBO Insert(NewBrickOrder brickOrder)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "OrderNo", brickOrder.OrderNo },
                { "BrickId", brickOrder.BrickId },
                { "BricksOrdered", brickOrder.BricksOrdered },
                { "OrderedDate", brickOrder.OrderedDate.ToUniversalTime() },
                { "ExpectedDate", brickOrder.ExpectedDate.ToUniversalTime() },
            };
            return Select<BrickOrderDBO>("brickorders_insert", parameters)?.FirstOrDefault();
        }

        public BrickOrderDBO Update(EditBrickOrder brickOrder)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", brickOrder.Id },
                { "OrderNo", brickOrder.OrderNo },
                { "BrickId", brickOrder.BrickId },
                { "BricksOrdered", brickOrder.BricksOrdered },
                { "OrderedDate", brickOrder.OrderedDate.ToUniversalTime() },
                { "ExpectedDate", brickOrder.ExpectedDate.ToUniversalTime() },
            };
            return Select<BrickOrderDBO>("brickorders_update", parameters)?.FirstOrDefault();
        }

        public BrickOrderDBO Cancel(long id, DateTime cancelledDate)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", id },
                { "CancelledDate", cancelledDate.ToUniversalTime() },
            };
            return Select<BrickOrderDBO>("brickorders_cancel", parameters)?.FirstOrDefault();
        }

        public BrickOrderDBO Uncancel(long id)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", id },
            };
            return Select<BrickOrderDBO>("brickorders_uncancel", parameters)?.FirstOrDefault();
        }

        public void Delete(long id)
        {
            Dictionary<string, object?> parameters = new()
            {
                { "Id", id },
            };
            ExecuteWithParameters("brickorders_delete", parameters);
        }
    }
}
