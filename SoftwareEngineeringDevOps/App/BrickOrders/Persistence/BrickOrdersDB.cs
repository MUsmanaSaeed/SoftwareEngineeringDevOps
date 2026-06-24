using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.BrickOrders.Persistence.DBOs;
using SoftwareEngineeringDevOps.App.Database;

namespace SoftwareEngineeringDevOps.App.BrickOrders.Persistence
{
    public class BrickOrdersDB : DB, IBrickOrdersDB
    {
        private readonly ILogger<BrickOrdersDB> _logger;

        public BrickOrdersDB(SQL_Execute sqlExecute, ILogger<BrickOrdersDB> logger) : base(sqlExecute, logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
        }

        public IEnumerable<BrickOrderDBO> ListAll()
        {
            try
            {
                var orders = Select<BrickOrderDBO>("brickorders_listall") ?? [];
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all brick orders");
                throw;
            }
        }

        public BrickOrderDBO? GetById(long id)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Id", id },
                };

                BrickOrderDBO? order = Select<BrickOrderDBO>("brickorders_getbyid", parameters)?.FirstOrDefault();

                if (order == null)
                {
                    _logger.LogWarning("Brick order not found with ID: {OrderId}", id);
                }

                return order;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve brick order by ID: {OrderId}", id);
                throw;
            }
        }

        public IEnumerable<BrickOrderDBO> GetByBrickId(long brickId)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "BrickId", brickId },
                };

                var orders = Select<BrickOrderDBO>("brickorders_getbybrickid", parameters) ?? [];
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve brick orders by brick ID: {BrickId}", brickId);
                throw;
            }
        }

        public BrickOrderDBO Insert(NewBrickOrder brickOrder)
        {
            ArgumentNullException.ThrowIfNull(brickOrder);
            ArgumentException.ThrowIfNullOrWhiteSpace(brickOrder.OrderNo);

            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "OrderNo", brickOrder.OrderNo },
                    { "BrickId", brickOrder.BrickId },
                    { "BricksOrdered", brickOrder.BricksOrdered },
                    { "OrderedDate", brickOrder.OrderedDate.ToUniversalTime() },
                    { "ExpectedDate", brickOrder.ExpectedDate.ToUniversalTime() },
                    { "CreatedById", brickOrder.CreatedById },
                };

                BrickOrderDBO? insertedOrder = Select<BrickOrderDBO>("brickorders_insert", parameters)?.FirstOrDefault();

                if (insertedOrder == null)
                {
                    _logger.LogError("Failed to insert brick order - database returned null: Order No: {OrderNo}", 
                        brickOrder.OrderNo);
                    throw new InvalidOperationException($"Failed to insert brick order: {brickOrder.OrderNo}");
                }

                return insertedOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert brick order: Order No: {OrderNo}", brickOrder.OrderNo);
                throw;
            }
        }

        public BrickOrderDBO Update(EditBrickOrder brickOrder)
        {
            ArgumentNullException.ThrowIfNull(brickOrder);

            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Id", brickOrder.Id },
                    { "BrickId", brickOrder.BrickId },
                    { "BricksOrdered", brickOrder.BricksOrdered },
                    { "OrderedDate", brickOrder.OrderedDate.ToUniversalTime() },
                    { "ExpectedDate", brickOrder.ExpectedDate.ToUniversalTime() },
                };

                BrickOrderDBO? updatedOrder = Select<BrickOrderDBO>("brickorders_update", parameters)?.FirstOrDefault();

                if (updatedOrder == null)
                {
                    _logger.LogError("Failed to update brick order - database returned null: {OrderId}", brickOrder.Id);
                    throw new InvalidOperationException($"Failed to update brick order: {brickOrder.Id}");
                }

                return updatedOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update brick order: {OrderId}", brickOrder.Id);
                throw;
            }
        }

        public BrickOrderDBO Cancel(long id, DateTime cancelledDate)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Id", id },
                    { "CancelledDate", cancelledDate.ToUniversalTime() },
                };

                BrickOrderDBO? cancelledOrder = Select<BrickOrderDBO>("brickorders_cancel", parameters)?.FirstOrDefault();

                if (cancelledOrder == null)
                {
                    _logger.LogError("Failed to cancel brick order - database returned null: {OrderId}", id);
                    throw new InvalidOperationException($"Failed to cancel brick order: {id}");
                }

                return cancelledOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel brick order: {OrderId}", id);
                throw;
            }
        }

        public BrickOrderDBO Uncancel(long id)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Id", id },
                };

                BrickOrderDBO? uncancelledOrder = Select<BrickOrderDBO>("brickorders_uncancel", parameters)?.FirstOrDefault();

                if (uncancelledOrder == null)
                {
                    _logger.LogError("Failed to uncancel brick order - database returned null: {OrderId}", id);
                    throw new InvalidOperationException($"Failed to uncancel brick order: {id}");
                }

                return uncancelledOrder;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to uncancel brick order: {OrderId}", id);
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

                ExecuteWithParameters("brickorders_delete", parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete brick order: {OrderId}", id);
                throw;
            }
        }
    }
}
