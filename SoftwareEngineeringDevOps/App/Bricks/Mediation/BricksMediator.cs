using SoftwareEngineeringDevOps.App.Bricks.Persistence;

namespace SoftwareEngineeringDevOps.App.Bricks
{
    public class BricksMediator : IBricksMediator
    {
        IBricksDB Db { get; }

        public BricksMediator(IBricksDB db)
        {
            Db = db;
        }

        public async Task<IEnumerable<IBrick>> GetAllBricks()
        {
            await Task.CompletedTask;
            return Db.ListAll().Select(Brick.FromDBO);
        }

        public async Task<IBrick?> GetBrickById(long id)
        {
            await Task.CompletedTask;
            var dbo = Db.GetById(id);
            return dbo == null ? null : Brick.FromDBO(dbo);
        }

        public async Task<IBrick> Insert(NewBrick brick)
        {
            await Task.CompletedTask;
            var dbo = Db.Insert(brick);
            return Brick.FromDBO(dbo);
        }

        public async Task<IBrick> Update(EditBrick brick)
        {
            await Task.CompletedTask;
            var dbo = Db.Update(brick);
            return Brick.FromDBO(dbo);
        }

        public async Task Delete(long id)
        {
            await Task.CompletedTask;
            Db.Delete(id);
        }
    }
}
