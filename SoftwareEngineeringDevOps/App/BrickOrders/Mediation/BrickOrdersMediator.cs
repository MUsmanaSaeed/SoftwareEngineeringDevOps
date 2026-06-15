using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.BrickOrders.Repository;

namespace SoftwareEngineeringDevOps.App.BrickOrders
{
    public class BrickOrdersMediator : IBrickOrdersMediator
    {
        private readonly IBrickOrdersRepository _repository;
        private readonly ILogger<BrickOrdersMediator> _logger;

        public BrickOrdersMediator(IBrickOrdersRepository repository, ILogger<BrickOrdersMediator> logger)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(logger);
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<IBrickOrder>> GetAllBrickOrders()
        {
            try
            {
                await Task.CompletedTask;
                var orders = _repository.ListAll();
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to get all brick orders");
                throw;
            }
        }

        public async Task<IBrickOrder?> GetBrickOrderById(long id)
        {
            try
            {
                await Task.CompletedTask;
                var order = _repository.GetById(id);
                if (order == null)
                    _logger.LogWarning("Mediator: Brick order not found: {OrderId}", id);
                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to get brick order by ID: {OrderId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<IBrickOrder>> GetBrickOrdersByBrickId(long brickId)
        {
            try
            {
                await Task.CompletedTask;
                var orders = _repository.GetByBrickId(brickId);
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to get brick orders by brick ID: {BrickId}", brickId);
                throw;
            }
        }

        public async Task<IBrickOrder> Insert(NewBrickOrder brickOrder)
        {
            ArgumentNullException.ThrowIfNull(brickOrder);
            try
            {
                await Task.CompletedTask;
                var created = _repository.Insert(brickOrder);
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to insert brick order: Order No: {OrderNo}", brickOrder.OrderNo);
                throw;
            }
        }

        public async Task<IBrickOrder> Update(EditBrickOrder brickOrder)
        {
            ArgumentNullException.ThrowIfNull(brickOrder);
            try
            {
                await Task.CompletedTask;
                var updated = _repository.Update(brickOrder);
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to update brick order: {OrderId}", brickOrder.Id);
                throw;
            }
        }

        public async Task<IBrickOrder> Cancel(long id)
        {
            try
            {
                await Task.CompletedTask;
                var cancelled = _repository.Cancel(id, DateTime.UtcNow);
                return cancelled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to cancel brick order: {OrderId}", id);
                throw;
            }
        }

        public async Task<IBrickOrder> Uncancel(long id)
        {
            try
            {
                await Task.CompletedTask;
                var uncancelled = _repository.Uncancel(id);
                return uncancelled;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to uncancel brick order: {OrderId}", id);
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
                _logger.LogError(ex, "Mediator: Failed to delete brick order: {OrderId}", id);
                throw;
            }
        }
    }
}
