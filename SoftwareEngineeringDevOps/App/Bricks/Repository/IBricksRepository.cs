namespace SoftwareEngineeringDevOps.App.Bricks.Repository
{
    public interface IBricksRepository
    {
        IEnumerable<IBrick> ListAll();
        IBrick? GetById(long id);
        IEnumerable<IBrick> GetByManufacturerId(long manufacturerId);
        IBrick Insert(NewBrick brick);
        IBrick Update(EditBrick brick);
        void Delete(long id);
    }
}
