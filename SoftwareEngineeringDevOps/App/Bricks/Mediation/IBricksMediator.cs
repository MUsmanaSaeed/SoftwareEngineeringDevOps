namespace SoftwareEngineeringDevOps.App.Bricks
{
    public interface IBricksMediator
    {
        Task<IEnumerable<IBrick>> GetAllBricks();
        Task<IBrick?> GetBrickById(long id);
        Task<IEnumerable<IBrick>> GetBricksByManufacturerId(long manufacturerId);
        Task<IBrick> Insert(NewBrick brick);
        Task<IBrick> Update(EditBrick brick);
        Task Delete(long id);
    }
}
