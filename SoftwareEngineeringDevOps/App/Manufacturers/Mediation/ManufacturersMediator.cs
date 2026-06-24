using Microsoft.Extensions.Logging;
using SoftwareEngineeringDevOps.App.Manufacturers.Repository;

namespace SoftwareEngineeringDevOps.App.Manufacturers
{
    public class ManufacturersMediator : IManufacturersMediator
    {
        private readonly IManufacturersRepository _repository;
        private readonly ILogger<ManufacturersMediator> _logger;

        public ManufacturersMediator(IManufacturersRepository repository, ILogger<ManufacturersMediator> logger)
        {
            ArgumentNullException.ThrowIfNull(repository);
            ArgumentNullException.ThrowIfNull(logger);
            _repository = repository;
            _logger = logger;
        }

        public async Task<IEnumerable<IManufacturer>> GetAllManufacturers()
        {
            try
            {
                await Task.CompletedTask;
                var manufacturers = _repository.ListAll();
                return manufacturers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to get all manufacturers");
                throw;
            }
        }

        public async Task<IManufacturer?> GetManufacturerById(long id)
        {
            try
            {
                await Task.CompletedTask;
                var manufacturer = _repository.GetById(id);

                if (manufacturer == null)
                {
                    _logger.LogWarning("Mediator: Manufacturer not found: {ManufacturerId}", id);
                }

                return manufacturer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to get manufacturer by ID: {ManufacturerId}", id);
                throw;
            }
        }

        public async Task<IManufacturer> Insert(NewManufacturer manufacturer)
        {
            ArgumentNullException.ThrowIfNull(manufacturer);

            try
            {
                await Task.CompletedTask;
                var created = _repository.Insert(manufacturer);
                return created;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to insert manufacturer: {ManufacturerName}", manufacturer.Name);
                throw;
            }
        }

        public async Task<IManufacturer> Update(EditManufacturer manufacturer)
        {
            ArgumentNullException.ThrowIfNull(manufacturer);

            try
            {
                await Task.CompletedTask;
                var updated = _repository.Update(manufacturer);
                return updated;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Mediator: Failed to update manufacturer: {ManufacturerId} - {ManufacturerName}", 
                    manufacturer.Id, manufacturer.Name);
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
                _logger.LogError(ex, "Mediator: Failed to delete manufacturer: {ManufacturerId}", id);
                throw;
            }
        }
    }
}
