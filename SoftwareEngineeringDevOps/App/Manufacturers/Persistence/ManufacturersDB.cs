using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.Database;
using SoftwareEngineeringDevOps.App.Manufacturers.Persistence.DBOs;

namespace SoftwareEngineeringDevOps.App.Manufacturers.Persistence
{
    public class ManufacturersDB : DB, IManufacturersDB
    {
        private readonly ILogger<ManufacturersDB> _logger;

        public ManufacturersDB(SQL_Execute sqlExecute, ILogger<ManufacturersDB> logger) : base(sqlExecute, logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
        }

        public IEnumerable<ManufacturerDBO> ListAll()
        {
            try
            {
                var manufacturers = Select<ManufacturerDBO>("manufacturers_listall") ?? [];
                return manufacturers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all manufacturers");
                throw;
            }
        }

        public ManufacturerDBO? GetById(long id)
        {
            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Id", id },
                };

                ManufacturerDBO? manufacturer = Select<ManufacturerDBO>("manufacturers_getbyid", parameters)?.FirstOrDefault();

                if (manufacturer == null)
                {
                    _logger.LogWarning("Manufacturer not found with ID: {ManufacturerId}", id);
                }

                return manufacturer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve manufacturer by ID: {ManufacturerId}", id);
                throw;
            }
        }

        public ManufacturerDBO Insert(NewManufacturer manufacturer)
        {
            ArgumentNullException.ThrowIfNull(manufacturer);
            ArgumentException.ThrowIfNullOrWhiteSpace(manufacturer.Name);

            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Name", manufacturer.Name },
                    { "Address1", manufacturer.Address1 },
                    { "Address2", manufacturer.Address2 },
                    { "Postcode", manufacturer.Postcode },
                    { "PhoneNo", manufacturer.PhoneNo },
                    { "Email", manufacturer.Email },
                };

                ManufacturerDBO? insertedManufacturer = Select<ManufacturerDBO>("manufacturers_insert", parameters)?.FirstOrDefault();

                if (insertedManufacturer == null)
                {
                    _logger.LogError("Failed to insert manufacturer - database returned null: {ManufacturerName}", 
                        manufacturer.Name);
                    throw new InvalidOperationException($"Failed to insert manufacturer: {manufacturer.Name}");
                }

                return insertedManufacturer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to insert manufacturer: {ManufacturerName}", manufacturer.Name);
                throw;
            }
        }

        public ManufacturerDBO Update(EditManufacturer manufacturer)
        {
            ArgumentNullException.ThrowIfNull(manufacturer);
            ArgumentException.ThrowIfNullOrWhiteSpace(manufacturer.Name);

            try
            {
                Dictionary<string, object?> parameters = new()
                {
                    { "Id", manufacturer.Id },
                    { "Name", manufacturer.Name },
                    { "Address1", manufacturer.Address1 },
                    { "Address2", manufacturer.Address2 },
                    { "Postcode", manufacturer.Postcode },
                    { "PhoneNo", manufacturer.PhoneNo },
                    { "Email", manufacturer.Email },
                };

                ManufacturerDBO? updatedManufacturer = Select<ManufacturerDBO>("manufacturers_update", parameters)?.FirstOrDefault();

                if (updatedManufacturer == null)
                {
                    _logger.LogError("Failed to update manufacturer - database returned null: {ManufacturerId} - {ManufacturerName}", 
                        manufacturer.Id, manufacturer.Name);
                    throw new InvalidOperationException($"Failed to update manufacturer: {manufacturer.Id}");
                }

                return updatedManufacturer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update manufacturer: {ManufacturerId} - {ManufacturerName}", 
                    manufacturer.Id, manufacturer.Name);
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

                ExecuteWithParameters("manufacturers_delete", parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete manufacturer: {ManufacturerId}", id);
                throw;
            }
        }
    }
}
