using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.BrickOrdersReceived.Persistence.DBOs;
using SoftwareEngineeringDevOps.App.Database;

namespace SoftwareEngineeringDevOps.App.BrickOrdersReceived.Persistence
{
    public class BrickOrdersReceivedDB : DB, IBrickOrdersReceivedDB
    {
        private readonly ILogger<BrickOrdersReceivedDB> _logger;

        public BrickOrdersReceivedDB(SQL_Execute sqlExecute, ILogger<BrickOrdersReceivedDB> logger) : base(sqlExecute, logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
        }

        public IEnumerable<BrickOrderReceivedDBO> ListAll()
        {
            try
            {
                var ordersReceived = Select<BrickOrderReceivedDBO>("brickordersreceived_listall") ?? [];
                return ordersReceived;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all brick orders received");
                throw;
            }
        }

        public BrickOrderReceivedDBO? GetById(long id)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Id", id },
                };

                BrickOrderReceivedDBO? received = Select<BrickOrderReceivedDBO>("brickordersreceived_getbyid", parameters)?.FirstOrDefault();

                if (received == null)
                {
                    _logger.LogWarning("Brick order received not found with ID: {ReceivedId}", id);
                }

                return received;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve brick order received by ID: {ReceivedId}", id);
                throw;
            }
        }

        public IEnumerable<BrickOrderReceivedDBO> GetByBrickOrderId(long brickOrderId)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "BrickOrderId", brickOrderId },
                };

                var ordersReceived = Select<BrickOrderReceivedDBO>("brickordersreceived_getbybrickorderid", parameters) ?? [];
                return ordersReceived;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve brick orders received by order ID: {BrickOrderId}", brickOrderId);
                throw;
            }
        }

        public BrickOrderReceivedDBO Insert(NewBrickOrderReceived brickOrderReceived)
        {
            ArgumentNullException.ThrowIfNull(brickOrderReceived);

            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "BrickOrderId", brickOrderReceived.BrickOrderId },
                    { "BricksReceived", brickOrderReceived.BricksReceived },
                    { "ReceivedDate", brickOrderReceived.ReceivedDate.ToUniversalTime() },
                    { "ReceivedById", brickOrderReceived.ReceivedById },
                };

                BrickOrderReceivedDBO? insertedReceived = Select<BrickOrderReceivedDBO>("brickordersreceived_insert", parameters)?.FirstOrDefault();

                if (insertedReceived == null)
                {
                    _logger.LogError("Failed to insert brick order received - database returned null: Order ID: {BrickOrderId}", 
                        brickOrderReceived.BrickOrderId);
                    throw new InvalidOperationException($"Failed to insert brick order received for order: {brickOrderReceived.BrickOrderId}");
                }

                return insertedReceived;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert brick order received: Order ID: {BrickOrderId}", 
                    brickOrderReceived.BrickOrderId);
                throw;
            }
        }

        public BrickOrderReceivedDBO Update(EditBrickOrderReceived brickOrderReceived)
        {
            ArgumentNullException.ThrowIfNull(brickOrderReceived);

            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Id", brickOrderReceived.Id },
                    { "BricksReceived", brickOrderReceived.BricksReceived },
                    { "ReceivedDate", brickOrderReceived.ReceivedDate.ToUniversalTime() },
                };

                BrickOrderReceivedDBO? updatedReceived = Select<BrickOrderReceivedDBO>("brickordersreceived_update", parameters)?.FirstOrDefault();

                if (updatedReceived == null)
                {
                    _logger.LogError("Failed to update brick order received - database returned null: {ReceivedId}", 
                        brickOrderReceived.Id);
                    throw new InvalidOperationException($"Failed to update brick order received: {brickOrderReceived.Id}");
                }

                return updatedReceived;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update brick order received: {ReceivedId}", brickOrderReceived.Id);
                throw;
            }
        }

        public void Delete(long id)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Id", id },
                };

                ExecuteWithParameters("brickordersreceived_delete", parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete brick order received: {ReceivedId}", id);
                throw;
            }
        }
    }
}
