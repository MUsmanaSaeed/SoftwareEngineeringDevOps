using FluentAssertions;
using Moq;
using SoftwareEngineeringDevOps.App.Auth;
using SoftwareEngineeringDevOps.App.Bricks;
using SoftwareEngineeringDevOps.App.BrickOrders;
using SoftwareEngineeringDevOps.App.BrickOrdersReceived;
using SoftwareEngineeringDevOps.App.Users;
using SoftwareEngineeringDevOps.Components.ViewModels;
using SoftwareEngineeringDevOps.Tests.TestUtilities;

namespace SoftwareEngineeringDevOps.Tests.UnitTests.ViewModels
{
    /// <summary>
    /// Comprehensive tests for OrdersViewModel covering order loading, selection, filtering, received tracking, and fulfillment calculations
    /// </summary>
    public class OrdersViewModelTests
    {
        private readonly Mock<IBrickOrdersMediator> _mockOrdersMediator;
        private readonly Mock<IBrickOrdersReceivedMediator> _mockReceivedMediator;
        private readonly Mock<IBricksMediator> _mockBricksMediator;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly OrdersViewModel _viewModel;

        public OrdersViewModelTests()
        {
            _mockOrdersMediator = new Mock<IBrickOrdersMediator>();
            _mockReceivedMediator = new Mock<IBrickOrdersReceivedMediator>();
            _mockBricksMediator = new Mock<IBricksMediator>();
            _mockAuthService = new Mock<IAuthService>();

            _mockAuthService.Setup(a => a.CurrentUser).Returns(MockDataFactory.Users.CreateAdmin());

            _viewModel = new OrdersViewModel(
                _mockOrdersMediator.Object,
                _mockReceivedMediator.Object,
                _mockBricksMediator.Object,
                _mockAuthService.Object
            );
        }

        #region LoadOrders Tests

        [Fact]
        public async Task LoadOrders_ShouldLoadOrdersBricksAndReceived_WhenCalled()
        {
            // Arrange
            var orders = new List<IBrickOrder>
            {
                MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-20260101-001"),
                MockDataFactory.BrickOrders.CreateValid(2, 2, "ORD-20260101-002")
            };
            var bricks = new List<IBrick>
            {
                MockDataFactory.Bricks.CreateValid(1),
                MockDataFactory.Bricks.CreateValid(2)
            };
            var received = new List<IBrickOrderReceived>
            {
                MockDataFactory.BrickOrdersReceived.CreateValid(1, 1, 500)
            };

            _mockOrdersMediator.Setup(m => m.GetAllBrickOrders()).ReturnsAsync(orders);
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(bricks);
            _mockReceivedMediator.Setup(m => m.GetAllBrickOrdersReceived()).ReturnsAsync(received);

            // Act
            await _viewModel.LoadOrders();

            // Assert
            _viewModel.AllOrders.Should().HaveCount(2);
            _viewModel.Bricks.Should().HaveCount(2);
            _viewModel.IsLoading.Should().BeFalse();
            _viewModel.OrderGroups.Should().HaveCount(2);
        }

        [Fact]
        public async Task LoadOrders_ShouldGroupOrdersByOrderNo()
        {
            // Arrange
            var orders = new List<IBrickOrder>
            {
                MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-20260101-001"),
                MockDataFactory.BrickOrders.CreateValid(2, 1, "ORD-20260101-001"),
                MockDataFactory.BrickOrders.CreateValid(3, 2, "ORD-20260101-002")
            };
            var bricks = new List<IBrick> { MockDataFactory.Bricks.CreateValid(1), MockDataFactory.Bricks.CreateValid(2) };
            var received = Enumerable.Empty<IBrickOrderReceived>();

            _mockOrdersMediator.Setup(m => m.GetAllBrickOrders()).ReturnsAsync(orders);
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(bricks);
            _mockReceivedMediator.Setup(m => m.GetAllBrickOrdersReceived()).ReturnsAsync(received);

            // Act
            await _viewModel.LoadOrders();

            // Assert
            _viewModel.OrderGroups.Should().HaveCount(2);
            _viewModel.OrderGroups.ElementAt(0).Should().HaveCount(2);
            _viewModel.OrderGroups.ElementAt(1).Should().HaveCount(1);
        }

        [Fact]
        public async Task LoadOrders_ShouldSetCurrentUserRole()
        {
            // Arrange
            var adminUser = MockDataFactory.Users.CreateValid(1, "admin", isAdmin: true);
            _mockAuthService.Setup(a => a.CurrentUser).Returns(adminUser);

            _mockOrdersMediator.Setup(m => m.GetAllBrickOrders()).ReturnsAsync(Enumerable.Empty<IBrickOrder>());
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(Enumerable.Empty<IBrick>());
            _mockReceivedMediator.Setup(m => m.GetAllBrickOrdersReceived()).ReturnsAsync(Enumerable.Empty<IBrickOrderReceived>());

            // Act
            await _viewModel.LoadOrders();

            // Assert
            _viewModel.CurrentUserRole.Should().Be(UserRole.Admin);
        }

        #endregion

        #region ApplyOrderGroupSelection Tests

        [Fact]
        public async Task ApplyOrderGroupSelection_ShouldSelectOrderGroupByOrderNo()
        {
            // Arrange
            var orders = new List<IBrickOrder>
            {
                MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-001"),
                MockDataFactory.BrickOrders.CreateValid(2, 1, "ORD-001"),
                MockDataFactory.BrickOrders.CreateValid(3, 2, "ORD-002")
            };
            var bricks = new List<IBrick> { MockDataFactory.Bricks.CreateValid(1), MockDataFactory.Bricks.CreateValid(2) };
            var received = Enumerable.Empty<IBrickOrderReceived>();

            _mockOrdersMediator.Setup(m => m.GetAllBrickOrders()).ReturnsAsync(orders);
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(bricks);
            _mockReceivedMediator.Setup(m => m.GetAllBrickOrdersReceived()).ReturnsAsync(received);

            await _viewModel.LoadOrders();

            // Act
            _viewModel.ApplyOrderGroupSelection("ORD-001");

            // Assert
            _viewModel.SelectedOrderNo.Should().Be("ORD-001");
            _viewModel.SelectedOrderLines.Should().HaveCount(2);
        }

        [Fact]
        public async Task ApplyOrderGroupSelection_ShouldClearExpandedRowsWhenSelectingDifferentGroup()
        {
            // Arrange
            var orders = new List<IBrickOrder>
            {
                MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-001"),
                MockDataFactory.BrickOrders.CreateValid(2, 1, "ORD-001"),
                MockDataFactory.BrickOrders.CreateValid(3, 2, "ORD-002")
            };
            var bricks = new List<IBrick> { MockDataFactory.Bricks.CreateValid(1), MockDataFactory.Bricks.CreateValid(2) };
            var received = Enumerable.Empty<IBrickOrderReceived>();

            _mockOrdersMediator.Setup(m => m.GetAllBrickOrders()).ReturnsAsync(orders);
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(bricks);
            _mockReceivedMediator.Setup(m => m.GetAllBrickOrdersReceived()).ReturnsAsync(received);

            await _viewModel.LoadOrders();
            _viewModel.ApplyOrderGroupSelection("ORD-001");
            _viewModel.ToggleRowExpansion(1);

            // Act
            _viewModel.ApplyOrderGroupSelection("ORD-002");

            // Assert
            _viewModel.ExpandedRows.Should().BeEmpty();
        }

        #endregion

        #region SelectOrderGroup Tests

        [Fact]
        public async Task SelectOrderGroup_ShouldSelectAndLoadReceivedItems()
        {
            // Arrange
            var orders = new List<IBrickOrder>
            {
                MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-001"),
                MockDataFactory.BrickOrders.CreateValid(2, 1, "ORD-002")
            };
            var bricks = new List<IBrick> { MockDataFactory.Bricks.CreateValid(1) };
            var received = new List<IBrickOrderReceived>
            {
                MockDataFactory.BrickOrdersReceived.CreateValid(1, 1, 500),
                MockDataFactory.BrickOrdersReceived.CreateValid(2, 2, 300)
            };

            _mockOrdersMediator.Setup(m => m.GetAllBrickOrders()).ReturnsAsync(orders);
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(bricks);
            _mockReceivedMediator.Setup(m => m.GetAllBrickOrdersReceived()).ReturnsAsync(received);

            await _viewModel.LoadOrders();

            // Act
            await _viewModel.SelectOrderGroup("ORD-001");

            // Assert
            _viewModel.SelectedOrderNo.Should().Be("ORD-001");
            _viewModel.ReceivedByOrderLine.Should().ContainKey(1);
        }

        #endregion

        #region ToggleRowExpansion Tests

        [Fact]
        public void ToggleRowExpansion_ShouldExpandRow_WhenNotExpanded()
        {
            // Act
            _viewModel.ToggleRowExpansion(1);

            // Assert
            _viewModel.IsRowExpanded(1).Should().BeTrue();
        }

        [Fact]
        public void ToggleRowExpansion_ShouldCollapseRow_WhenExpanded()
        {
            // Arrange
            _viewModel.ToggleRowExpansion(1);

            // Act
            _viewModel.ToggleRowExpansion(1);

            // Assert
            _viewModel.IsRowExpanded(1).Should().BeFalse();
        }

        #endregion

        #region FilteredOrderGroups Tests

        [Fact]
        public async Task FilteredOrderGroups_ShouldReturnAllGroups_WhenSearchTermIsEmpty()
        {
            // Arrange
            var orders = new List<IBrickOrder>
            {
                MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-001"),
                MockDataFactory.BrickOrders.CreateValid(2, 2, "ORD-002"),
                MockDataFactory.BrickOrders.CreateValid(3, 1, "ORD-003")
            };
            var bricks = new List<IBrick> { MockDataFactory.Bricks.CreateValid(1), MockDataFactory.Bricks.CreateValid(2) };
            var received = Enumerable.Empty<IBrickOrderReceived>();

            _mockOrdersMediator.Setup(m => m.GetAllBrickOrders()).ReturnsAsync(orders);
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(bricks);
            _mockReceivedMediator.Setup(m => m.GetAllBrickOrdersReceived()).ReturnsAsync(received);

            await _viewModel.LoadOrders();
            _viewModel.OrderGroupsSearchTerm = string.Empty;

            // Act
            var result = _viewModel.FilteredOrderGroups;

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task FilteredOrderGroups_ShouldFilterByOrderNo()
        {
            // Arrange
            var orders = new List<IBrickOrder>
            {
                MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-001"),
                MockDataFactory.BrickOrders.CreateValid(2, 2, "ORD-002"),
                MockDataFactory.BrickOrders.CreateValid(3, 1, "ORD-003")
            };
            var bricks = new List<IBrick> { MockDataFactory.Bricks.CreateValid(1), MockDataFactory.Bricks.CreateValid(2) };
            var received = Enumerable.Empty<IBrickOrderReceived>();

            _mockOrdersMediator.Setup(m => m.GetAllBrickOrders()).ReturnsAsync(orders);
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(bricks);
            _mockReceivedMediator.Setup(m => m.GetAllBrickOrdersReceived()).ReturnsAsync(received);

            await _viewModel.LoadOrders();
            _viewModel.OrderGroupsSearchTerm = "001";

            // Act
            var result = _viewModel.FilteredOrderGroups;

            // Assert
            result.Should().HaveCount(1);
            result.First().Key.Should().Contain("001");
        }

        #endregion

        #region GetFulfillmentPercentage Tests

        [Fact]
        public void GetFulfillmentPercentage_ShouldReturnZero_WhenNoReceivedItems()
        {
            // Arrange
            var order = MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-001");

            // Act
            var percentage = _viewModel.GetFulfillmentPercentage(order);

            // Assert
            percentage.Should().Be(0);
        }

        [Fact]
        public void GetFulfillmentPercentage_ShouldCalculateCorrectly_WhenPartiallyReceived()
        {
            // Arrange
            var order = MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-001");
            var received = MockDataFactory.BrickOrdersReceived.CreateValid(1, 1, 500);
            _viewModel.ReceivedByOrderLine[1] = new List<IBrickOrderReceived> { received };

            // Act
            var percentage = _viewModel.GetFulfillmentPercentage(order);

            // Assert
            // order.BricksOrdered is 1000, received is 500 = 50%
            percentage.Should().Be(50);
        }

        [Fact]
        public void GetFulfillmentPercentage_ShouldReturnZero_WhenBricksOrderedIsZero()
        {
            // Arrange
            var order = MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-001");
            var received = MockDataFactory.BrickOrdersReceived.CreateValid(1, 1, 500);
            _viewModel.ReceivedByOrderLine[1] = new List<IBrickOrderReceived> { received };

            // Act - Create order with 0 bricks ordered
            var zeroOrder = new Mock<IBrickOrder>();
            zeroOrder.Setup(o => o.BricksOrdered).Returns(0);
            zeroOrder.Setup(o => o.Id).Returns(1);

            var percentage = _viewModel.GetFulfillmentPercentage(zeroOrder.Object);

            // Assert
            percentage.Should().Be(0);
        }

        #endregion

        #region GetTotalReceived Tests

        [Fact]
        public void GetTotalReceived_ShouldReturnZero_WhenNoReceivedItems()
        {
            // Arrange
            var order = MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-001");

            // Act
            var total = _viewModel.GetTotalReceived(order);

            // Assert
            total.Should().Be(0);
        }

        [Fact]
        public void GetTotalReceived_ShouldReturnTotalOfAllReceivedItems()
        {
            // Arrange
            var order = MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-001");
            var received1 = MockDataFactory.BrickOrdersReceived.CreateValid(1, 1, 300);
            var received2 = MockDataFactory.BrickOrdersReceived.CreateValid(2, 1, 200);
            _viewModel.ReceivedByOrderLine[1] = new List<IBrickOrderReceived> { received1, received2 };

            // Act
            var total = _viewModel.GetTotalReceived(order);

            // Assert
            total.Should().Be(500);
        }

        #endregion

        #region HasReceivedItems Tests

        [Fact]
        public void HasReceivedItems_ShouldReturnTrue_WhenReceivedItemsExist()
        {
            // Arrange
            var received = MockDataFactory.BrickOrdersReceived.CreateValid(1, 1, 500);
            _viewModel.ReceivedByOrderLine[1] = new List<IBrickOrderReceived> { received };

            // Act
            var hasItems = _viewModel.HasReceivedItems(1);

            // Assert
            hasItems.Should().BeTrue();
        }

        [Fact]
        public void HasReceivedItems_ShouldReturnFalse_WhenNoReceivedItems()
        {
            // Act
            var hasItems = _viewModel.HasReceivedItems(1);

            // Assert
            hasItems.Should().BeFalse();
        }

        #endregion

        #region IsOrderFullyReceived Tests

        [Fact]
        public void IsOrderFullyReceived_ShouldReturnTrue_WhenReceivedEqualsOrdered()
        {
            // Arrange
            var order = MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-001");
            var received = MockDataFactory.BrickOrdersReceived.CreateValid(1, 1, 1000);
            _viewModel.ReceivedByOrderLine[1] = new List<IBrickOrderReceived> { received };

            // Act
            var isFullyReceived = _viewModel.IsOrderFullyReceived(order);

            // Assert
            isFullyReceived.Should().BeTrue();
        }

        [Fact]
        public void IsOrderFullyReceived_ShouldReturnFalse_WhenReceivedLessThanOrdered()
        {
            // Arrange
            var order = MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-001");
            var received = MockDataFactory.BrickOrdersReceived.CreateValid(1, 1, 500);
            _viewModel.ReceivedByOrderLine[1] = new List<IBrickOrderReceived> { received };

            // Act
            var isFullyReceived = _viewModel.IsOrderFullyReceived(order);

            // Assert
            isFullyReceived.Should().BeFalse();
        }

        #endregion

        #region CurrentUserId Tests

        [Fact]
        public void CurrentUserId_ShouldReturnCurrentUserIdFromAuthService()
        {
            // Arrange
            var user = MockDataFactory.Users.CreateValid(5, "testuser");
            _mockAuthService.Setup(a => a.CurrentUser).Returns(user);

            // Act
            var userId = _viewModel.CurrentUserId;

            // Assert
            userId.Should().Be(5);
        }

        [Fact]
        public void CurrentUserId_ShouldReturnZero_WhenNoCurrentUser()
        {
            // Arrange
            _mockAuthService.Setup(a => a.CurrentUser).Returns((IUserInfo?)null);

            // Act
            var userId = _viewModel.CurrentUserId;

            // Assert
            userId.Should().Be(0);
        }

        #endregion

        #region Property Initialization Tests

        [Fact]
        public void OrdersViewModel_ShouldInitializeWithEmptyCollections()
        {
            // Act & Assert
            _viewModel.AllOrders.Should().BeEmpty();
            _viewModel.Bricks.Should().BeEmpty();
            _viewModel.OrderGroups.Should().BeEmpty();
            _viewModel.SelectedOrderLines.Should().BeEmpty();
        }

        [Fact]
        public void OrdersViewModel_ShouldInitializeWithValidationErrorsEmpty()
        {
            // Act & Assert
            _viewModel.ValidationErrors.Should().BeEmpty();
        }

        [Fact]
        public void OrdersViewModel_ShouldInitializeWithEmptyErrorMessage()
        {
            // Act & Assert
            _viewModel.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void OrdersViewModel_ShouldInitializeWithIsLoadingFalse()
        {
            // Act & Assert
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public void OrdersViewModel_ShouldInitializeWithAllModalsHidden()
        {
            // Act & Assert
            _viewModel.ShowAddOrderModal.Should().BeFalse();
            _viewModel.ShowEditOrderModal.Should().BeFalse();
            _viewModel.ShowDeleteOrderConfirm.Should().BeFalse();
            _viewModel.ShowAddReceivedModal.Should().BeFalse();
            _viewModel.ShowEditReceivedModal.Should().BeFalse();
            _viewModel.ShowDeleteReceivedConfirm.Should().BeFalse();
        }

        [Fact]
        public void OrdersViewModel_ShouldInitializeWithEmptySearchTerms()
        {
            // Act & Assert
            _viewModel.OrderGroupsSearchTerm.Should().Be(string.Empty);
            _viewModel.ReceivedSearchTerm.Should().Be(string.Empty);
        }

        #endregion

        #region SelectedOrder Aggregates Tests

        [Fact]
        public async Task SelectedOrderTotalOrdered_ShouldSumBricksOrderedForSelectedOrderLines()
        {
            // Arrange
            var orders = new List<IBrickOrder>
            {
                MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-001"),
                MockDataFactory.BrickOrders.CreateValid(2, 1, "ORD-001")
            };
            var bricks = new List<IBrick> { MockDataFactory.Bricks.CreateValid(1) };
            var received = Enumerable.Empty<IBrickOrderReceived>();

            _mockOrdersMediator.Setup(m => m.GetAllBrickOrders()).ReturnsAsync(orders);
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(bricks);
            _mockReceivedMediator.Setup(m => m.GetAllBrickOrdersReceived()).ReturnsAsync(received);

            await _viewModel.LoadOrders();
            await _viewModel.SelectOrderGroup("ORD-001");

            // Act
            var total = _viewModel.SelectedOrderTotalOrdered;

            // Assert
            total.Should().Be(2000); // 1000 + 1000
        }

        [Fact]
        public async Task SelectedOrderFirstOrderedDate_ShouldReturnEarliestDate()
        {
            // Arrange
            var orders = new List<IBrickOrder>
            {
                MockDataFactory.BrickOrders.CreateValid(1, 1, "ORD-001"),
                MockDataFactory.BrickOrders.CreateValid(2, 1, "ORD-001")
            };
            var bricks = new List<IBrick> { MockDataFactory.Bricks.CreateValid(1) };
            var received = Enumerable.Empty<IBrickOrderReceived>();

            _mockOrdersMediator.Setup(m => m.GetAllBrickOrders()).ReturnsAsync(orders);
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(bricks);
            _mockReceivedMediator.Setup(m => m.GetAllBrickOrdersReceived()).ReturnsAsync(received);

            await _viewModel.LoadOrders();
            await _viewModel.SelectOrderGroup("ORD-001");

            // Act
            var date = _viewModel.SelectedOrderFirstOrderedDate;

            // Assert
            date.Should().NotBeNull();
        }

        #endregion
    }
}
