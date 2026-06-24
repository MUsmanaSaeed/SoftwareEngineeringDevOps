using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftwareEngineeringDevOps.App.Bricks;
using SoftwareEngineeringDevOps.App.Bricks.Repository;
using SoftwareEngineeringDevOps.Tests.TestUtilities;

namespace SoftwareEngineeringDevOps.Tests.UnitTests.Mediators
{
    /// <summary>
    /// Unit tests for BricksMediator covering CRUD operations with mocked dependencies
    /// </summary>
    public class BricksMediatorTests
    {
        private readonly Mock<IBricksRepository> _mockRepository;
        private readonly Mock<ILogger<BricksMediator>> _mockLogger;
        private readonly BricksMediator _mediator;

        public BricksMediatorTests()
        {
            _mockRepository = new Mock<IBricksRepository>();
            _mockLogger = new Mock<ILogger<BricksMediator>>();
            _mediator = new BricksMediator(_mockRepository.Object, _mockLogger.Object);
        }

        #region GetAllBricks Tests

        [Fact]
        public async Task GetAllBricks_ShouldReturnAllBricks_WhenBricksExist()
        {
            // Arrange
            var expectedBricks = new List<IBrick>
            {
                MockDataFactory.Bricks.CreateValid(1, 1, "Brick 1"),
                MockDataFactory.Bricks.CreateValid(2, 1, "Brick 2"),
                MockDataFactory.Bricks.CreateValid(3, 1, "Brick 3")
            };
            _mockRepository.Setup(r => r.ListAll()).Returns(expectedBricks);

            // Act
            var result = await _mediator.GetAllBricks();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedBricks);
            _mockRepository.Verify(r => r.ListAll(), Times.Once);
        }

        [Fact]
        public async Task GetAllBricks_ShouldReturnEmptyCollection_WhenNoBricksExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.ListAll()).Returns(Enumerable.Empty<IBrick>());

            // Act
            var result = await _mediator.GetAllBricks();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockRepository.Verify(r => r.ListAll(), Times.Once);
        }

        #endregion

        #region GetBrickById Tests

        [Fact]
        public async Task GetBrickById_ShouldReturnBrick_WhenBrickExists()
        {
            // Arrange
            const long brickId = 1;
            var expectedBrick = MockDataFactory.Bricks.CreateValid(brickId);
            _mockRepository.Setup(r => r.GetById(brickId)).Returns(expectedBrick);

            // Act
            var result = await _mediator.GetBrickById(brickId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedBrick);
            _mockRepository.Verify(r => r.GetById(brickId), Times.Once);
        }

        [Fact]
        public async Task GetBrickById_ShouldReturnNull_WhenBrickDoesNotExist()
        {
            // Arrange
            const long nonExistentId = 999;
            _mockRepository.Setup(r => r.GetById(nonExistentId)).Returns((IBrick?)null);

            // Act
            var result = await _mediator.GetBrickById(nonExistentId);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetById(nonExistentId), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(long.MinValue)]
        public async Task GetBrickById_ShouldHandleInvalidIds(long invalidId)
        {
            // Arrange
            _mockRepository.Setup(r => r.GetById(invalidId)).Returns((IBrick?)null);

            // Act
            var result = await _mediator.GetBrickById(invalidId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetBricksByManufacturerId Tests

        [Fact]
        public async Task GetBricksByManufacturerId_ShouldReturnBricks_WhenBricksExist()
        {
            // Arrange
            const long manufacturerId = 1;
            var expectedBricks = new List<IBrick>
            {
                MockDataFactory.Bricks.CreateValid(1, manufacturerId, "Brick 1"),
                MockDataFactory.Bricks.CreateValid(2, manufacturerId, "Brick 2")
            };
            _mockRepository.Setup(r => r.GetByManufacturerId(manufacturerId)).Returns(expectedBricks);

            // Act
            var result = await _mediator.GetBricksByManufacturerId(manufacturerId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedBricks);
            _mockRepository.Verify(r => r.GetByManufacturerId(manufacturerId), Times.Once);
        }

        [Fact]
        public async Task GetBricksByManufacturerId_ShouldReturnEmpty_WhenNoBricksForManufacturer()
        {
            // Arrange
            const long manufacturerId = 999;
            _mockRepository.Setup(r => r.GetByManufacturerId(manufacturerId)).Returns(Enumerable.Empty<IBrick>());

            // Act
            var result = await _mediator.GetBricksByManufacturerId(manufacturerId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region Insert Tests

        [Fact]
        public async Task Insert_ShouldReturnCreatedBrick_WhenValidDataProvided()
        {
            // Arrange
            var newBrick = MockDataFactory.Bricks.CreateValidNew();
            var expectedBrick = MockDataFactory.Bricks.CreateValid(1, newBrick.ManufacturerId, newBrick.Name);
            _mockRepository.Setup(r => r.Insert(newBrick)).Returns(expectedBrick);

            // Act
            var result = await _mediator.Insert(newBrick);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(newBrick.Name);
            result.Price.Should().Be(newBrick.Price);
            _mockRepository.Verify(r => r.Insert(newBrick), Times.Once);
        }

        [Fact]
        public async Task Insert_ShouldHandleMinimumValues()
        {
            // Arrange
            var newBrick = MockDataFactory.Bricks.CreateValidNewWithMinimumValues();
            var expectedBrick = MockDataFactory.Bricks.CreateValid(1, newBrick.ManufacturerId, newBrick.Name);
            _mockRepository.Setup(r => r.Insert(newBrick)).Returns(expectedBrick);

            // Act
            var result = await _mediator.Insert(newBrick);

            // Assert
            result.Should().NotBeNull();
            _mockRepository.Verify(r => r.Insert(newBrick), Times.Once);
        }

        [Fact]
        public async Task Insert_ShouldHandleMaximumValues()
        {
            // Arrange
            var newBrick = MockDataFactory.Bricks.CreateValidNewWithMaximumValues();
            var expectedBrick = MockDataFactory.Bricks.CreateValid(1, newBrick.ManufacturerId, newBrick.Name);
            _mockRepository.Setup(r => r.Insert(newBrick)).Returns(expectedBrick);

            // Act
            var result = await _mediator.Insert(newBrick);

            // Assert
            result.Should().NotBeNull();
            _mockRepository.Verify(r => r.Insert(newBrick), Times.Once);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_ShouldReturnUpdatedBrick_WhenValidDataProvided()
        {
            // Arrange
            var existingBrick = MockDataFactory.Bricks.CreateValid(1);
            var editBrick = new EditBrick(existingBrick)
            {
                Name = "Updated Brick Name",
                Price = 15.75m
            };
            var updatedBrick = MockDataFactory.Bricks.CreateValid(1, 1, "Updated Brick Name");
            _mockRepository.Setup(r => r.Update(editBrick)).Returns(updatedBrick);

            // Act
            var result = await _mediator.Update(editBrick);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Brick Name");
            _mockRepository.Verify(r => r.Update(editBrick), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldPreserveId_WhenUpdatingBrick()
        {
            // Arrange
            const long brickId = 5;
            var existingBrick = MockDataFactory.Bricks.CreateValid(brickId);
            var editBrick = new EditBrick(existingBrick);
            var updatedBrick = MockDataFactory.Bricks.CreateValid(brickId);
            _mockRepository.Setup(r => r.Update(editBrick)).Returns(updatedBrick);

            // Act
            var result = await _mediator.Update(editBrick);

            // Assert
            result.Id.Should().Be(brickId);
            _mockRepository.Verify(r => r.Update(editBrick), Times.Once);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ShouldCallRepositoryDelete_WhenValidIdProvided()
        {
            // Arrange
            const long brickId = 1;
            _mockRepository.Setup(r => r.Delete(brickId));

            // Act
            await _mediator.Delete(brickId);

            // Assert
            _mockRepository.Verify(r => r.Delete(brickId), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldNotThrow_WhenDeletingNonExistentBrick()
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

        #endregion
    }
}
