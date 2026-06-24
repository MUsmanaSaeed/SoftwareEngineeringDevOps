using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.BrickOrdersReceived.Repository;

namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived
{
    public class BrickOrdersReceivedMediator : IBrickOrdersReceivedMediator
    {
        private readonly IBrickOrdersReceivedRepository _repository;
        private readonly ILogger<BrickOrdersReceivedMediator> _logger;

        public BrickOrdersReceivedMediator(IBrickOrdersReceivedRepository repository, ILogger<BrickOrdersReceivedMediator> logger)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(logger);
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<IBrickOrderReceived>> GetAllBrickOrdersReceived()
        {
            try
            {
                await Task.CompletedTask;
                var ordersReceived = _repository.ListAll();
                return ordersReceived;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to get all brick orders received");
                throw;
            }
        }

        public async Task<IBrickOrderReceived?> GetBrickOrderReceivedById(long id)
        {
            try
            {
                await Task.CompletedTask;
                var received = _repository.GetById(id);
                if (received == null)
                    _logger.LogWarning("Mediator: Brick order received not found: {ReceivedId}", id);
                return received;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to get brick order received by ID: {ReceivedId}", id);
                throw;
            }
        }

        public async Task<IEnumerable<IBrickOrderReceived>> GetBrickOrdersReceivedByBrickOrderId(long brickOrderId)
        {
            try
            {
                await Task.CompletedTask;
                var ordersReceived = _repository.GetByBrickOrderId(brickOrderId);
                return ordersReceived;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to get brick orders received by order ID: {BrickOrderId}", brickOrderId);
                throw;
            }
        }

        public async Task<IBrickOrderReceived> Insert(NewBrickOrderReceived brickOrderReceived)
        {
            ArgumentNullException.ThrowIfNull(brickOrderReceived);
            try
            {
                await Task.CompletedTask;
                var created = _repository.Insert(brickOrderReceived);
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to insert brick order received: Order ID: {BrickOrderId}", brickOrderReceived.BrickOrderId);
                throw;
            }
        }

        public async Task<IBrickOrderReceived> Update(EditBrickOrderReceived brickOrderReceived)
        {
            ArgumentNullException.ThrowIfNull(brickOrderReceived);
            try
            {
                await Task.CompletedTask;
                var updated = _repository.Update(brickOrderReceived);
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to update brick order received: {ReceivedId}", brickOrderReceived.Id);
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
                _logger.LogError(ex, "Mediator: Failed to delete brick order received: {ReceivedId}", id);
                throw;
            }
        }
    }
}
