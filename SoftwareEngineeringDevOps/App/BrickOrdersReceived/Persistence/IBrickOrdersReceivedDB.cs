using SoftwareEngineeringDevOps.App.BrickOrdersReceived.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived.Persistence
{
    public interface IBrickOrdersReceivedDB
    {
        IEnumerable<BrickOrderReceivedDBO> ListAll();
        BrickOrderReceivedDBO? GetById(long id);
        BrickOrderReceivedDBO Insert(NewBrickOrderReceived brickOrderReceived);
        BrickOrderReceivedDBO Update(EditBrickOrderReceived brickOrderReceived);
        void Delete(long id);
    }
}
