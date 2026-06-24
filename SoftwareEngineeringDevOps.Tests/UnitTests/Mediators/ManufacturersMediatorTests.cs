using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftwareEngineeringDevOps.App.Manufacturers;
using SoftwareEngineeringDevOps.App.Manufacturers.Repository;
using SoftwareEngineeringDevOps.Tests.TestUtilities;

namespace SoftwareEngineeringDevOps.Tests.UnitTests.Mediators
{
    /// <summary>
    /// Unit tests for ManufacturersMediator covering CRUD operations with mocked dependencies
    /// </summary>
    public class ManufacturersMediatorTests
    {
        private readonly Mock<IManufacturersRepository> _mockRepository;
        private readonly Mock<ILogger<ManufacturersMediator>> _mockLogger;
        private readonly ManufacturersMediator _mediator;

        public ManufacturersMediatorTests()
        {
            _mockRepository = new Mock<IManufacturersRepository>();
            _mockLogger = new Mock<ILogger<ManufacturersMediator>>();
            _mediator = new ManufacturersMediator(_mockRepository.Object, _mockLogger.Object);
        }

        #region GetAllManufacturers Tests

        [Fact]
        public async Task GetAllManufacturers_ShouldReturnAllManufacturers_WhenManufacturersExist()
        {
            // Arrange
            var expectedManufacturers = new List<IManufacturer>
            {
                MockDataFactory.Manufacturers.CreateValid(1, "Acme Bricks"),
                MockDataFactory.Manufacturers.CreateValid(2, "Premier Clay"),
                MockDataFactory.Manufacturers.CreateValid(3, "Standard Bricks Ltd")
            };
            _mockRepository.Setup(r => r.ListAll()).Returns(expectedManufacturers);

            // Act
            var result = await _mediator.GetAllManufacturers();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedManufacturers);
            _mockRepository.Verify(r => r.ListAll(), Times.Once);
        }

        [Fact]
        public async Task GetAllManufacturers_ShouldReturnEmptyCollection_WhenNoManufacturersExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.ListAll()).Returns(Enumerable.Empty<IManufacturer>());

            // Act
            var result = await _mediator.GetAllManufacturers();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockRepository.Verify(r => r.ListAll(), Times.Once);
        }

        #endregion

        #region GetManufacturerById Tests

        [Fact]
        public async Task GetManufacturerById_ShouldReturnManufacturer_WhenManufacturerExists()
        {
            // Arrange
            const long manufacturerId = 1;
            var expectedManufacturer = MockDataFactory.Manufacturers.CreateValid(manufacturerId, "Acme Bricks");
            _mockRepository.Setup(r => r.GetById(manufacturerId)).Returns(expectedManufacturer);

            // Act
            var result = await _mediator.GetManufacturerById(manufacturerId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedManufacturer);
            _mockRepository.Verify(r => r.GetById(manufacturerId), Times.Once);
        }

        [Fact]
        public async Task GetManufacturerById_ShouldReturnNull_WhenManufacturerDoesNotExist()
        {
            // Arrange
            const long nonExistentId = 999;
            _mockRepository.Setup(r => r.GetById(nonExistentId)).Returns((IManufacturer?)null);

            // Act
            var result = await _mediator.GetManufacturerById(nonExistentId);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetById(nonExistentId), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(long.MinValue)]
        public async Task GetManufacturerById_ShouldHandleInvalidIds(long invalidId)
        {
            // Arrange
            _mockRepository.Setup(r => r.GetById(invalidId)).Returns((IManufacturer?)null);

            // Act
            var result = await _mediator.GetManufacturerById(invalidId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Insert Tests

        [Fact]
        public async Task Insert_ShouldReturnCreatedManufacturer_WhenValidDataProvided()
        {
            // Arrange
            var newManufacturer = MockDataFactory.Manufacturers.CreateValidNew("New Manufacturer");
            var expectedManufacturer = MockDataFactory.Manufacturers.CreateValid(1, "New Manufacturer");
            _mockRepository.Setup(r => r.Insert(newManufacturer)).Returns(expectedManufacturer);

            // Act
            var result = await _mediator.Insert(newManufacturer);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(newManufacturer.Name);
            result.Address1.Should().Be(newManufacturer.Address1);
            result.Postcode.Should().Be(newManufacturer.Postcode);
            _mockRepository.Verify(r => r.Insert(newManufacturer), Times.Once);
        }

        [Fact]
        public async Task Insert_ShouldPreserveContactDetails_WhenInsertingManufacturer()
        {
            // Arrange
            var newManufacturer = MockDataFactory.Manufacturers.CreateValidNew("Contact Test");
            var expectedManufacturer = MockDataFactory.Manufacturers.CreateValid(1, "Contact Test");
            _mockRepository.Setup(r => r.Insert(newManufacturer)).Returns(expectedManufacturer);

            // Act
            var result = await _mediator.Insert(newManufacturer);

            // Assert
            result.PhoneNo.Should().Be(newManufacturer.PhoneNo);
            result.Email.Should().Be(newManufacturer.Email);
            _mockRepository.Verify(r => r.Insert(newManufacturer), Times.Once);
        }

        [Fact]
        public async Task Insert_ShouldPreserveAddressDetails_WhenInsertingManufacturer()
        {
            // Arrange
            var newManufacturer = MockDataFactory.Manufacturers.CreateValidNew("Address Test");
            var expectedManufacturer = MockDataFactory.Manufacturers.CreateValid(1, "Address Test");
            _mockRepository.Setup(r => r.Insert(newManufacturer)).Returns(expectedManufacturer);

            // Act
            var result = await _mediator.Insert(newManufacturer);

            // Assert
            result.Address1.Should().Be(newManufacturer.Address1);
            result.Address2.Should().Be(newManufacturer.Address2);
            result.Postcode.Should().Be(newManufacturer.Postcode);
            _mockRepository.Verify(r => r.Insert(newManufacturer), Times.Once);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_ShouldReturnUpdatedManufacturer_WhenValidDataProvided()
        {
            // Arrange
            var existingManufacturer = MockDataFactory.Manufacturers.CreateValid(1, "Original Name");
            var editManufacturer = new EditManufacturer(existingManufacturer)
            {
                Name = "Updated Name",
                PhoneNo = "+441234567890"
            };
            var updatedManufacturer = MockDataFactory.Manufacturers.CreateValid(1, "Updated Name");
            _mockRepository.Setup(r => r.Update(editManufacturer)).Returns(updatedManufacturer);

            // Act
            var result = await _mediator.Update(editManufacturer);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Name");
            _mockRepository.Verify(r => r.Update(editManufacturer), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldPreserveId_WhenUpdatingManufacturer()
        {
            // Arrange
            const long manufacturerId = 5;
            var existingManufacturer = MockDataFactory.Manufacturers.CreateValid(manufacturerId);
            var editManufacturer = new EditManufacturer(existingManufacturer);
            var updatedManufacturer = MockDataFactory.Manufacturers.CreateValid(manufacturerId);
            _mockRepository.Setup(r => r.Update(editManufacturer)).Returns(updatedManufacturer);

            // Act
            var result = await _mediator.Update(editManufacturer);

            // Assert
            result.Id.Should().Be(manufacturerId);
            _mockRepository.Verify(r => r.Update(editManufacturer), Times.Once);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ShouldCallRepositoryDelete_WhenValidIdProvided()
        {
            // Arrange
            const long manufacturerId = 1;
            _mockRepository.Setup(r => r.Delete(manufacturerId));

            // Act
            await _mediator.Delete(manufacturerId);

            // Assert
            _mockRepository.Verify(r => r.Delete(manufacturerId), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldNotThrow_WhenDeletingNonExistentManufacturer()
        {
            // Arrange
            const long nonExistentId = 999;
            _mockRepository.Setup(r => r.Delete(nonExistentId));

            // Act
            Func<Task> act = async () => await _mediator.Delete(nonExistentId);

            // Assert
            await act.Should().NotThrowAsync();
            _mockRepository.Verify(r => r.Delete(nonExistentId), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Delete_ShouldHandleInvalidIds(long invalidId)
        {
            // Arrange
            _mockRepository.Setup(r => r.Delete(invalidId));

            // Act
            Func<Task> act = async () => await _mediator.Delete(invalidId);

            // Assert
            await act.Should().NotThrowAsync();
        }

        #endregion
    }
}
