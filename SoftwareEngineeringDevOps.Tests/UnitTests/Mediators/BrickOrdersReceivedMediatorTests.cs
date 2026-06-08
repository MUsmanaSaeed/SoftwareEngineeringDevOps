using FluentAssertions;
using Moq;
using SoftwareEngineeringDevOps.App.BrickOrdersReceived;
using SoftwareEngineeringDevOps.App.BrickOrdersReceived.Repository;
using SoftwareEngineeringDevOps.Tests.TestUtilities;

namespace SoftwareEngineeringDevOps.Tests.UnitTests.Mediators
{
    /// <summary>
    /// Unit tests for BrickOrdersReceivedMediator covering CRUD operations with mocked dependencies
    /// </summary>
    public class BrickOrdersReceivedMediatorTests
    {
        private readonly Mock<IBrickOrdersReceivedRepository> _mockRepository;
        private readonly BrickOrdersReceivedMediator _mediator;

        public BrickOrdersReceivedMediatorTests()
        {
            _mockRepository = new Mock<IBrickOrdersReceivedRepository>();
            _mediator = new BrickOrdersReceivedMediator(_mockRepository.Object);
        }

        #region GetAllBrickOrdersReceived Tests

        [Fact]
        public async Task GetAllBrickOrdersReceived_ShouldReturnAllReceivedBrickOrders_WhenOrdersExist()
        {
            // Arrange
            var expectedReceivedOrders = new List<IBrickOrderReceived>
            {
                MockDataFactory.BrickOrdersReceived.CreateValid(1, 1, 500),
                MockDataFactory.BrickOrdersReceived.CreateValid(2, 2, 300),
                MockDataFactory.BrickOrdersReceived.CreateValid(3, 1, 200)
            };
            _mockRepository.Setup(r => r.ListAll()).Returns(expectedReceivedOrders);

            // Act
            var result = await _mediator.GetAllBrickOrdersReceived();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedReceivedOrders);
            _mockRepository.Verify(r => r.ListAll(), Times.Once);
        }

        [Fact]
        public async Task GetAllBrickOrdersReceived_ShouldReturnEmptyCollection_WhenNoOrdersExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.ListAll()).Returns(Enumerable.Empty<IBrickOrderReceived>());

            // Act
            var result = await _mediator.GetAllBrickOrdersReceived();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockRepository.Verify(r => r.ListAll(), Times.Once);
        }

        #endregion

        #region GetBrickOrderReceivedById Tests

        [Fact]
        public async Task GetBrickOrderReceivedById_ShouldReturnBrickOrderReceived_WhenOrderExists()
        {
            // Arrange
            const long receivedId = 1;
            var expectedReceivedOrder = MockDataFactory.BrickOrdersReceived.CreateValid(receivedId);
            _mockRepository.Setup(r => r.GetById(receivedId)).Returns(expectedReceivedOrder);

            // Act
            var result = await _mediator.GetBrickOrderReceivedById(receivedId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedReceivedOrder);
            _mockRepository.Verify(r => r.GetById(receivedId), Times.Once);
        }

        [Fact]
        public async Task GetBrickOrderReceivedById_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            // Arrange
            const long nonExistentId = 999;
            _mockRepository.Setup(r => r.GetById(nonExistentId)).Returns((IBrickOrderReceived?)null);

            // Act
            var result = await _mediator.GetBrickOrderReceivedById(nonExistentId);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetById(nonExistentId), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(long.MinValue)]
        public async Task GetBrickOrderReceivedById_ShouldHandleInvalidIds(long invalidId)
        {
            // Arrange
            _mockRepository.Setup(r => r.GetById(invalidId)).Returns((IBrickOrderReceived?)null);

            // Act
            var result = await _mediator.GetBrickOrderReceivedById(invalidId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetBrickOrdersReceivedByBrickOrderId Tests

        [Fact]
        public async Task GetBrickOrdersReceivedByBrickOrderId_ShouldReturnReceivedOrders_WhenOrdersExistForBrickOrder()
        {
            // Arrange
            const long brickOrderId = 1;
            var expectedReceivedOrders = new List<IBrickOrderReceived>
            {
                MockDataFactory.BrickOrdersReceived.CreateValid(1, brickOrderId, 500),
                MockDataFactory.BrickOrdersReceived.CreateValid(2, brickOrderId, 300),
                MockDataFactory.BrickOrdersReceived.CreateValid(3, brickOrderId, 200)
            };
            _mockRepository.Setup(r => r.GetByBrickOrderId(brickOrderId)).Returns(expectedReceivedOrders);

            // Act
            var result = await _mediator.GetBrickOrdersReceivedByBrickOrderId(brickOrderId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedReceivedOrders);
            _mockRepository.Verify(r => r.GetByBrickOrderId(brickOrderId), Times.Once);
        }

        [Fact]
        public async Task GetBrickOrdersReceivedByBrickOrderId_ShouldReturnEmpty_WhenNoReceivedOrdersForBrickOrder()
        {
            // Arrange
            const long brickOrderId = 999;
            _mockRepository.Setup(r => r.GetByBrickOrderId(brickOrderId)).Returns(Enumerable.Empty<IBrickOrderReceived>());

            // Act
            var result = await _mediator.GetBrickOrdersReceivedByBrickOrderId(brickOrderId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockRepository.Verify(r => r.GetByBrickOrderId(brickOrderId), Times.Once);
        }

        #endregion

        #region Insert Tests

        [Fact]
        public async Task Insert_ShouldReturnCreatedBrickOrderReceived_WhenValidDataProvided()
        {
            // Arrange
            var newReceivedOrder = MockDataFactory.BrickOrdersReceived.CreateValidNew(1, 500);
            var expectedReceivedOrder = MockDataFactory.BrickOrdersReceived.CreateValid(1, 1, 500);
            _mockRepository.Setup(r => r.Insert(newReceivedOrder)).Returns(expectedReceivedOrder);

            // Act
            var result = await _mediator.Insert(newReceivedOrder);

            // Assert
            result.Should().NotBeNull();
            result.BrickOrderId.Should().Be(newReceivedOrder.BrickOrderId);
            result.BricksReceived.Should().Be(newReceivedOrder.BricksReceived);
            _mockRepository.Verify(r => r.Insert(newReceivedOrder), Times.Once);
        }

        [Fact]
        public async Task Insert_ShouldPreserveReceivedDetails_WhenInsertingReceivedOrder()
        {
            // Arrange
            var newReceivedOrder = MockDataFactory.BrickOrdersReceived.CreateValidNew(2, 750);
            var expectedReceivedOrder = MockDataFactory.BrickOrdersReceived.CreateValid(1, 2, 750);
            _mockRepository.Setup(r => r.Insert(newReceivedOrder)).Returns(expectedReceivedOrder);

            // Act
            var result = await _mediator.Insert(newReceivedOrder);

            // Assert
            result.ReceivedDate.Should().BeSameDateAs(newReceivedOrder.ReceivedDate);
            _mockRepository.Verify(r => r.Insert(newReceivedOrder), Times.Once);
        }

        [Fact]
        public async Task Insert_ShouldPreserveReceivedByUser_WhenInsertingReceivedOrder()
        {
            // Arrange
            var newReceivedOrder = MockDataFactory.BrickOrdersReceived.CreateValidNew(1, 500);
            var expectedReceivedOrder = MockDataFactory.BrickOrdersReceived.CreateValid(1, 1, 500);
            _mockRepository.Setup(r => r.Insert(newReceivedOrder)).Returns(expectedReceivedOrder);

            // Act
            var result = await _mediator.Insert(newReceivedOrder);

            // Assert
            result.ReceivedBy.Should().NotBeNull();
            _mockRepository.Verify(r => r.Insert(newReceivedOrder), Times.Once);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_ShouldReturnUpdatedBrickOrderReceived_WhenValidDataProvided()
        {
            // Arrange
            var existingReceivedOrder = MockDataFactory.BrickOrdersReceived.CreateValid(1, 1, 500);
            var editReceivedOrder = new EditBrickOrderReceived(existingReceivedOrder);
            var updatedReceivedOrder = MockDataFactory.BrickOrdersReceived.CreateValid(1, 1, 500);
            _mockRepository.Setup(r => r.Update(editReceivedOrder)).Returns(updatedReceivedOrder);

            // Act
            var result = await _mediator.Update(editReceivedOrder);

            // Assert
            result.Should().NotBeNull();
            result.BrickOrderId.Should().Be(existingReceivedOrder.BrickOrderId);
            _mockRepository.Verify(r => r.Update(editReceivedOrder), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldPreserveId_WhenUpdatingReceivedOrder()
        {
            // Arrange
            const long receivedId = 5;
            var existingReceivedOrder = MockDataFactory.BrickOrdersReceived.CreateValid(receivedId, 1, 500);
            var editReceivedOrder = new EditBrickOrderReceived(existingReceivedOrder);
            var updatedReceivedOrder = MockDataFactory.BrickOrdersReceived.CreateValid(receivedId, 1, 500);
            _mockRepository.Setup(r => r.Update(editReceivedOrder)).Returns(updatedReceivedOrder);

            // Act
            var result = await _mediator.Update(editReceivedOrder);

            // Assert
            result.Id.Should().Be(receivedId);
            _mockRepository.Verify(r => r.Update(editReceivedOrder), Times.Once);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ShouldCallRepositoryDelete_WhenValidIdProvided()
        {
            // Arrange
            const long receivedId = 1;
            _mockRepository.Setup(r => r.Delete(receivedId));

            // Act
            await _mediator.Delete(receivedId);

            // Assert
            _mockRepository.Verify(r => r.Delete(receivedId), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldNotThrow_WhenDeletingNonExistentReceivedOrder()
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
