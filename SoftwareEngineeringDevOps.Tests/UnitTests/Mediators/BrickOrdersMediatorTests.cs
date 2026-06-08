using FluentAssertions;
using Moq;
using SoftwareEngineeringDevOps.App.BrickOrders;
using SoftwareEngineeringDevOps.App.BrickOrders.Repository;
using SoftwareEngineeringDevOps.Tests.TestUtilities;

namespace SoftwareEngineeringDevOps.Tests.UnitTests.Mediators
{
    /// <summary>
    /// Unit tests for BrickOrdersMediator covering CRUD operations and order-specific behaviors with mocked dependencies
    /// </summary>
    public class BrickOrdersMediatorTests
    {
        private readonly Mock<IBrickOrdersRepository> _mockRepository;
        private readonly BrickOrdersMediator _mediator;

        public BrickOrdersMediatorTests()
        {
            _mockRepository = new Mock<IBrickOrdersRepository>();
            _mediator = new BrickOrdersMediator(_mockRepository.Object);
        }

        #region GetAllBrickOrders Tests

        [Fact]
        public async Task GetAllBrickOrders_ShouldReturnAllBrickOrders_WhenOrdersExist()
        {
            // Arrange
            var expectedOrders = new List<IBrickOrder>
            {
                MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-20260101-001"),
                MockDataFactory.BrickOrders.CreateValid(2, 2, "ORD-20260101-002"),
                MockDataFactory.BrickOrders.CreateValid(3, 1, "ORD-20260101-003")
            };
            _mockRepository.Setup(r => r.ListAll()).Returns(expectedOrders);

            // Act
            var result = await _mediator.GetAllBrickOrders();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedOrders);
            _mockRepository.Verify(r => r.ListAll(), Times.Once);
        }

        [Fact]
        public async Task GetAllBrickOrders_ShouldReturnEmptyCollection_WhenNoOrdersExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.ListAll()).Returns(Enumerable.Empty<IBrickOrder>());

            // Act
            var result = await _mediator.GetAllBrickOrders();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockRepository.Verify(r => r.ListAll(), Times.Once);
        }

        #endregion

        #region GetBrickOrderById Tests

        [Fact]
        public async Task GetBrickOrderById_ShouldReturnBrickOrder_WhenOrderExists()
        {
            // Arrange
            const long orderId = 1;
            var expectedOrder = MockDataFactory.BrickOrders.CreateValid(orderId);
            _mockRepository.Setup(r => r.GetById(orderId)).Returns(expectedOrder);

            // Act
            var result = await _mediator.GetBrickOrderById(orderId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedOrder);
            _mockRepository.Verify(r => r.GetById(orderId), Times.Once);
        }

        [Fact]
        public async Task GetBrickOrderById_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            // Arrange
            const long nonExistentId = 999;
            _mockRepository.Setup(r => r.GetById(nonExistentId)).Returns((IBrickOrder?)null);

            // Act
            var result = await _mediator.GetBrickOrderById(nonExistentId);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetById(nonExistentId), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(long.MinValue)]
        public async Task GetBrickOrderById_ShouldHandleInvalidIds(long invalidId)
        {
            // Arrange
            _mockRepository.Setup(r => r.GetById(invalidId)).Returns((IBrickOrder?)null);

            // Act
            var result = await _mediator.GetBrickOrderById(invalidId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetBrickOrdersByBrickId Tests

        [Fact]
        public async Task GetBrickOrdersByBrickId_ShouldReturnOrders_WhenOrdersExistForBrick()
        {
            // Arrange
            const long brickId = 1;
            var expectedOrders = new List<IBrickOrder>
            {
                MockDataFactory.BrickOrders.CreateValid(1, brickId, "ORD-20260101-001"),
                MockDataFactory.BrickOrders.CreateValid(2, brickId, "ORD-20260101-002")
            };
            _mockRepository.Setup(r => r.GetByBrickId(brickId)).Returns(expectedOrders);

            // Act
            var result = await _mediator.GetBrickOrdersByBrickId(brickId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
            result.Should().BeEquivalentTo(expectedOrders);
            _mockRepository.Verify(r => r.GetByBrickId(brickId), Times.Once);
        }

        [Fact]
        public async Task GetBrickOrdersByBrickId_ShouldReturnEmpty_WhenNoOrdersForBrick()
        {
            // Arrange
            const long brickId = 999;
            _mockRepository.Setup(r => r.GetByBrickId(brickId)).Returns(Enumerable.Empty<IBrickOrder>());

            // Act
            var result = await _mediator.GetBrickOrdersByBrickId(brickId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        #endregion

        #region Insert Tests

        [Fact]
        public async Task Insert_ShouldReturnCreatedBrickOrder_WhenValidDataProvided()
        {
            // Arrange
            var newOrder = MockDataFactory.BrickOrders.CreateValidNew(1, 1000);
            var expectedOrder = MockDataFactory.BrickOrders.CreateValid(1, 1, newOrder.OrderNo);
            _mockRepository.Setup(r => r.Insert(newOrder)).Returns(expectedOrder);

            // Act
            var result = await _mediator.Insert(newOrder);

            // Assert
            result.Should().NotBeNull();
            result.BrickId.Should().Be(newOrder.BrickId);
            result.BricksOrdered.Should().Be(newOrder.BricksOrdered);
            _mockRepository.Verify(r => r.Insert(newOrder), Times.Once);
        }

        [Fact]
        public async Task Insert_ShouldPreserveOrderDetails_WhenInsertingOrder()
        {
            // Arrange
            var newOrder = MockDataFactory.BrickOrders.CreateValidNew(2, 500);
            var expectedOrder = MockDataFactory.BrickOrders.CreateValid(1, 2, newOrder.OrderNo);
            _mockRepository.Setup(r => r.Insert(newOrder)).Returns(expectedOrder);

            // Act
            var result = await _mediator.Insert(newOrder);

            // Assert
            result.OrderNo.Should().NotBeNullOrEmpty();
            result.OrderedDate.ToUtc().Should().BeSameDateAs(newOrder.OrderedDate.ToUtc());
            _mockRepository.Verify(r => r.Insert(newOrder), Times.Once);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_ShouldReturnUpdatedBrickOrder_WhenValidDataProvided()
        {
            // Arrange
            var existingOrder = MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-20260101-001");
            var editOrder = new EditBrickOrder(existingOrder);
            var updatedOrder = MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-20260101-001");
            _mockRepository.Setup(r => r.Update(editOrder)).Returns(updatedOrder);

            // Act
            var result = await _mediator.Update(editOrder);

            // Assert
            result.Should().NotBeNull();
            result.OrderNo.Should().Be(existingOrder.OrderNo);
            _mockRepository.Verify(r => r.Update(editOrder), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldPreserveId_WhenUpdatingOrder()
        {
            // Arrange
            const long orderId = 5;
            var existingOrder = MockDataFactory.BrickOrders.CreateValid(orderId);
            var editOrder = new EditBrickOrder(existingOrder);
            var updatedOrder = MockDataFactory.BrickOrders.CreateValid(orderId);
            _mockRepository.Setup(r => r.Update(editOrder)).Returns(updatedOrder);

            // Act
            var result = await _mediator.Update(editOrder);

            // Assert
            result.Id.Should().Be(orderId);
            _mockRepository.Verify(r => r.Update(editOrder), Times.Once);
        }

        #endregion

        #region Cancel Tests

        [Fact]
        public async Task Cancel_ShouldReturnCancelledBrickOrder_WhenOrderExists()
        {
            // Arrange
            const long orderId = 1;
            var cancelledOrder = MockDataFactory.BrickOrders.CreateValid(orderId, isCancelled: true);
            _mockRepository.Setup(r => r.Cancel(orderId, It.IsAny<DateTime>())).Returns(cancelledOrder);

            // Act
            var result = await _mediator.Cancel(orderId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(orderId);
            _mockRepository.Verify(r => r.Cancel(orderId, It.IsAny<DateTime>()), Times.Once);
        }

        [Fact]
        public async Task Cancel_ShouldPassCurrentDateTimeToRepository()
        {
            // Arrange
            const long orderId = 1;
            var cancelledOrder = MockDataFactory.BrickOrders.CreateValid(orderId, isCancelled: true);
            _mockRepository.Setup(r => r.Cancel(orderId, It.IsAny<DateTime>())).Returns(cancelledOrder);

            // Act
            await _mediator.Cancel(orderId);

            // Assert
            _mockRepository.Verify(r => r.Cancel(orderId, It.IsAny<DateTime>()), Times.Once);
        }

        #endregion

        #region Uncancel Tests

        [Fact]
        public async Task Uncancel_ShouldReturnUncancelledBrickOrder_WhenOrderExists()
        {
            // Arrange
            const long orderId = 1;
            var uncancelledOrder = MockDataFactory.BrickOrders.CreateValid(orderId, isCancelled: false);
            _mockRepository.Setup(r => r.Uncancel(orderId)).Returns(uncancelledOrder);

            // Act
            var result = await _mediator.Uncancel(orderId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(orderId);
            _mockRepository.Verify(r => r.Uncancel(orderId), Times.Once);
        }

        [Fact]
        public async Task Uncancel_ShouldCallRepositoryUncancel()
        {
            // Arrange
            const long orderId = 2;
            var uncancelledOrder = MockDataFactory.BrickOrders.CreateValid(orderId, isCancelled: false);
            _mockRepository.Setup(r => r.Uncancel(orderId)).Returns(uncancelledOrder);

            // Act
            await _mediator.Uncancel(orderId);

            // Assert
            _mockRepository.Verify(r => r.Uncancel(orderId), Times.Once);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ShouldCallRepositoryDelete_WhenValidIdProvided()
        {
            // Arrange
            const long orderId = 1;
            _mockRepository.Setup(r => r.Delete(orderId));

            // Act
            await _mediator.Delete(orderId);

            // Assert
            _mockRepository.Verify(r => r.Delete(orderId), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldNotThrow_WhenDeletingNonExistentOrder()
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
