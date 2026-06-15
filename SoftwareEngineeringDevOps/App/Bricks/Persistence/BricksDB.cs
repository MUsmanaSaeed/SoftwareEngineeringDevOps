using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.Bricks.Persistence.DBOs;
using SoftwareEngineeringDevOps.App.Database;

namespace SoftwareEngineeringDevOps.App.Bricks.Persistence
{
    public class BricksDB : DB, IBricksDB
    {
        private readonly ILogger<BricksDB> _logger;

        public BricksDB(SQL_Execute sqlExecute, ILogger<BricksDB> logger) : base(sqlExecute, logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
        }

        public IEnumerable<BrickDBO> ListAll()
        {
            try
            {
                var bricks = Select<BrickDBO>("bricks_listall") ?? [];
                return bricks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all bricks");
                throw;
            }
        }

        public BrickDBO? GetById(long id)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Id", id },
                };

                BrickDBO? brick = Select<BrickDBO>("bricks_getbyid", parameters)?.FirstOrDefault();

                if (brick == null)
                {
                    _logger.LogWarning("Brick not found with ID: {BrickId}", id);
                }

                return brick;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve brick by ID: {BrickId}", id);
                throw;
            }
        }

        public IEnumerable<BrickDBO> GetByManufacturerId(long manufacturerId)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "ManufacturerId", manufacturerId },
                };

                var bricks = Select<BrickDBO>("bricks_getbymanufacturerid", parameters) ?? [];
                return bricks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve bricks by manufacturer ID: {ManufacturerId}", manufacturerId);
                throw;
            }
        }

        public BrickDBO Insert(NewBrick brick)
        {
            ArgumentNullException.ThrowIfNull(brick);
            ArgumentException.ThrowIfNullOrWhiteSpace(brick.Name);

            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Name", brick.Name },
                    { "ManufacturerId", brick.ManufacturerId },
                    { "Price", brick.Price },
                    { "Colour", brick.Colour },
                    { "Material", brick.Material },
                    { "Strength", brick.Strength },
                    { "Width", brick.Width },
                    { "Height", brick.Height },
                    { "Depth", brick.Depth },
                    { "Type", brick.Type },
                    { "Voids", brick.Voids },
                };

                BrickDBO? insertedBrick = Select<BrickDBO>("bricks_insert", parameters)?.FirstOrDefault();

                if (insertedBrick == null)
                {
                    _logger.LogError("Failed to insert brick - database returned null: {BrickName}", brick.Name);
                    throw new InvalidOperationException($"Failed to insert brick: {brick.Name}");
                }

                return insertedBrick;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert brick: {BrickName}", brick.Name);
                throw;
            }
        }

        public BrickDBO Update(EditBrick brick)
        {
            ArgumentNullException.ThrowIfNull(brick);
            ArgumentException.ThrowIfNullOrWhiteSpace(brick.Name);

            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Id", brick.Id },
                    { "Name", brick.Name },
                    { "ManufacturerId", brick.ManufacturerId },
                    { "Price", brick.Price },
                    { "Colour", brick.Colour },
                    { "Material", brick.Material },
                    { "Strength", brick.Strength },
                    { "Width", brick.Width },
                    { "Height", brick.Height },
                    { "Depth", brick.Depth },
                    { "Type", brick.Type },
                    { "Voids", brick.Voids },
                };

                BrickDBO? updatedBrick = Select<BrickDBO>("bricks_update", parameters)?.FirstOrDefault();

                if (updatedBrick == null)
                {
                    _logger.LogError("Failed to update brick - database returned null: {BrickId} - {BrickName}", 
                        brick.Id, brick.Name);
                    throw new InvalidOperationException($"Failed to update brick: {brick.Id}");
                }

                return updatedBrick;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update brick: {BrickId} - {BrickName}", brick.Id, brick.Name);
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

                ExecuteWithParameters("bricks_delete", parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete brick: {BrickId}", id);
                throw;
            }
        }
    }
}
