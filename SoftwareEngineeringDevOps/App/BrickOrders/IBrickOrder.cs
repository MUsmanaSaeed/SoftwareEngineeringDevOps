using SoftwareEngineeringDevOps.App.Users;

namespace SoftwareEngineeringDevOps.App.BrickOrders
{
    public interface IBrickOrder
    {
        long Id { get; }
        string OrderNo { get; }
        long BrickId { get; }
        int BricksOrdered { get; }
        DateTime OrderedDate { get; }
        DateTime ExpectedDate { get; }
        DateTime? CancelledDate { get; }
        IUserInfo CreatedBy { get; }
    }
}
