using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.Bricks.Repository;

namespace SoftwareEngineeringDevOps.App.Bricks
{
    public class BricksMediator : IBricksMediator
    {
        private readonly IBricksRepository _repository;
        private readonly ILogger<BricksMediator> _logger;

        public BricksMediator(IBricksRepository repository, ILogger<BricksMediator> logger)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(logger);
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<IBrick>> GetAllBricks()
        {
            try
            {
                await Task.CompletedTask;
                var bricks = _repository.ListAll();
                return bricks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to get all bricks");
                throw;
            }
        }

        public async Task<IBrick?> GetBrickById(long id)
        {
            try
            {
                await Task.CompletedTask;
                var brick = _repository.GetById(id);

                if (brick == null)
                {
                    _logger.LogWarning("Mediator: Brick not found: {BrickId}", id);
                }

                return brick;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to get brick by ID: {BrickId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<IBrick>> GetBricksByManufacturerId(long manufacturerId)
        {
            try
            {
                await Task.CompletedTask;
                var bricks = _repository.GetByManufacturerId(manufacturerId);
                return bricks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to get bricks by manufacturer ID: {ManufacturerId}", manufacturerId);
                throw;
            }
        }

        public async Task<IBrick> Insert(NewBrick brick)
        {
            ArgumentNullException.ThrowIfNull(brick);

            try
            {
                await Task.CompletedTask;
                var created = _repository.Insert(brick);
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to insert brick: {BrickName}", brick.Name);
                throw;
            }
        }

        public async Task<IBrick> Update(EditBrick brick)
        {
            ArgumentNullException.ThrowIfNull(brick);

            try
            {
                await Task.CompletedTask;
                var updated = _repository.Update(brick);
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to update brick: {BrickId} - {BrickName}", brick.Id, brick.Name);
                throw;
            }
        }

        public async Task Delete(long id)
        {
            try
            {
                await Task.CompletedTask;
                _repository.Delete(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to delete brick: {BrickId}", id);
                throw;
            }
        }
    }
}
