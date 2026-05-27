using SoftwareEngineeringDevOps.App.BrickOrders.Persistence.DBOs;
using SoftwareEngineeringDevOps.App.Users;

namespace SoftwareEngineeringDevOps.App.BrickOrders
{
    public class BrickOrder : IBrickOrder
    {
        public static BrickOrder FromDBO(BrickOrderDBO dbo, IUserInfo createdBy)
        {
            return new BrickOrder
            {
                Id = dbo.Id,
                OrderNo = dbo.OrderNo,
                BrickId = dbo.BrickId,
                BricksOrdered = dbo.BricksOrdered,
                OrderedDate = dbo.OrderedDate.ToUkTime(),
                ExpectedDate = dbo.ExpectedDate.ToUkTime(),
                CancelledDate = dbo.CancelledDate?.ToUkTime(),
                CreatedBy = createdBy,
            };
        }

        public long Id { get; private set; }
        public string OrderNo { get; private set; }
        public long BrickId { get; private set; }
        public int BricksOrdered { get; private set; }
        public DateTime OrderedDate { get; private set; }
        public DateTime ExpectedDate { get; private set; }
        public DateTime? CancelledDate { get; private set; }
        public IUserInfo CreatedBy { get; private set; }
    }
}
