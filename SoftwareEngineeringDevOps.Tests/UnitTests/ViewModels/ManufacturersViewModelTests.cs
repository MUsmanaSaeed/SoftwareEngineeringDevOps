using FluentAssertions;
using Moq;
using SoftwareEngineeringDevOps.App.Auth;
using SoftwareEngineeringDevOps.App.Bricks;
using SoftwareEngineeringDevOps.App.BrickOrders;
using SoftwareEngineeringDevOps.App.Manufacturers;
using SoftwareEngineeringDevOps.Components.ViewModels;
using SoftwareEngineeringDevOps.Tests.TestUtilities;

namespace SoftwareEngineeringDevOps.Tests.UnitTests.ViewModels
{
    /// <summary>
    /// Comprehensive tests for ManufacturersViewModel covering CRUD operations, filtering, deletion constraints, and validation
    /// </summary>
    public class ManufacturersViewModelTests
    {
        private readonly Mock<IManufacturersMediator> _mockManufacturersMediator;
        private readonly Mock<IBricksMediator> _mockBricksMediator;
        private readonly Mock<IBrickOrdersMediator> _mockOrdersMediator;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly ManufacturersViewModel _viewModel;

        public ManufacturersViewModelTests()
        {
            _mockManufacturersMediator = new Mock<IManufacturersMediator>();
            _mockBricksMediator = new Mock<IBricksMediator>();
            _mockOrdersMediator = new Mock<IBrickOrdersMediator>();
            _mockAuthService = new Mock<IAuthService>();

            _mockAuthService.Setup(a => a.CurrentUser).Returns(MockDataFactory.Users.CreateAdmin());

            _viewModel = new ManufacturersViewModel(
                _mockManufacturersMediator.Object,
                _mockBricksMediator.Object,
                _mockOrdersMediator.Object,
                _mockAuthService.Object
            );
        }

        #region LoadManufacturers Tests

        [Fact]
        public async Task LoadManufacturers_ShouldLoadAllManufacturers_WhenCalled()
        {
            // Arrange
            var manufacturers = new List<IManufacturer>
            {
                MockDataFactory.Manufacturers.CreateValid(1, "Acme"),
                MockDataFactory.Manufacturers.CreateValid(2, "Premier"),
                MockDataFactory.Manufacturers.CreateValid(3, "Standard")
            };
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(manufacturers);

            // Act
            await _viewModel.LoadManufacturers();

            // Assert
            _viewModel.Manufacturers.Should().HaveCount(3);
            _viewModel.IsLoading.Should().BeFalse();
            _mockManufacturersMediator.Verify(m => m.GetAllManufacturers(), Times.Once);
        }

        [Fact]
        public async Task LoadManufacturers_ShouldHandleEmptyCollection()
        {
            // Arrange
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(Enumerable.Empty<IManufacturer>());

            // Act
            await _viewModel.LoadManufacturers();

            // Assert
            _viewModel.Manufacturers.Should().BeEmpty();
            _viewModel.IsLoading.Should().BeFalse();
        }

        #endregion

        #region SelectManufacturer Tests

        [Fact]
        public async Task SelectManufacturer_ShouldSelectManufacturerAndLoadBricks()
        {
            // Arrange
            var manufacturer = MockDataFactory.Manufacturers.CreateValid(1, "Acme");
            var bricks = new List<IBrick>
            {
                MockDataFactory.Bricks.CreateValid(1, 1, "Brick 1"),
                MockDataFactory.Bricks.CreateValid(2, 1, "Brick 2")
            };
            _mockBricksMediator.Setup(m => m.GetBricksByManufacturerId(1)).ReturnsAsync(bricks);

            // Act
            await _viewModel.SelectManufacturer(manufacturer);

            // Assert
            _viewModel.SelectedManufacturer.Should().Be(manufacturer);
            _viewModel.SelectedManufacturerBricks.Should().HaveCount(2);
            _mockBricksMediator.Verify(m => m.GetBricksByManufacturerId(1), Times.Once);
        }

        [Fact]
        public async Task SelectManufacturer_ShouldLoadEmptyBricksList_WhenManufacturerHasNoBricks()
        {
            // Arrange
            var manufacturer = MockDataFactory.Manufacturers.CreateValid(1, "Acme");
            _mockBricksMediator.Setup(m => m.GetBricksByManufacturerId(1)).ReturnsAsync(Enumerable.Empty<IBrick>());

            // Act
            await _viewModel.SelectManufacturer(manufacturer);

            // Assert
            _viewModel.SelectedManufacturer.Should().Be(manufacturer);
            _viewModel.SelectedManufacturerBricks.Should().BeEmpty();
        }

        #endregion

        #region FilteredManufacturers Tests

        [Fact]
        public void FilteredManufacturers_ShouldReturnAllManufacturers_WhenSearchTermIsEmpty()
        {
            // Arrange
            var manufacturers = new List<IManufacturer>
            {
                MockDataFactory.Manufacturers.CreateValid(1, "Acme"),
                MockDataFactory.Manufacturers.CreateValid(2, "Premier"),
                MockDataFactory.Manufacturers.CreateValid(3, "Standard")
            };
            _viewModel.Manufacturers = manufacturers;
            _viewModel.ManufacturersSearchTerm = string.Empty;

            // Act
            var result = _viewModel.FilteredManufacturers;

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public void FilteredManufacturers_ShouldFilterByName()
        {
            // Arrange
            var manufacturers = new List<IManufacturer>
            {
                MockDataFactory.Manufacturers.CreateValid(1, "Acme"),
                MockDataFactory.Manufacturers.CreateValid(2, "Premier"),
                MockDataFactory.Manufacturers.CreateValid(3, "Standard")
            };
            _viewModel.Manufacturers = manufacturers;
            _viewModel.ManufacturersSearchTerm = "Premier";

            // Act
            var result = _viewModel.FilteredManufacturers;

            // Assert
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Premier");
        }

        [Fact]
        public void FilteredManufacturers_ShouldFilterByEmail()
        {
            // Arrange
            var manufacturers = new List<IManufacturer>
            {
                MockDataFactory.Manufacturers.CreateValid(1, "Acme"),
                MockDataFactory.Manufacturers.CreateValid(2, "Premier"),
                MockDataFactory.Manufacturers.CreateValid(3, "Standard")
            };
            _viewModel.Manufacturers = manufacturers;
            _viewModel.ManufacturersSearchTerm = "test@example.com";

            // Act
            var result = _viewModel.FilteredManufacturers;

            // Assert
            result.Should().HaveCount(3); // All have same email
        }

        [Fact]
        public void FilteredManufacturers_ShouldReturnOrderedByName()
        {
            // Arrange
            var manufacturers = new List<IManufacturer>
            {
                MockDataFactory.Manufacturers.CreateValid(1, "Zebra"),
                MockDataFactory.Manufacturers.CreateValid(2, "Acme"),
                MockDataFactory.Manufacturers.CreateValid(3, "Premier")
            };
            _viewModel.Manufacturers = manufacturers;
            _viewModel.ManufacturersSearchTerm = string.Empty;

            // Act
            var result = _viewModel.FilteredManufacturers.ToList();

            // Assert
            result[0].Name.Should().Be("Acme");
            result[1].Name.Should().Be("Premier");
            result[2].Name.Should().Be("Zebra");
        }

        #endregion

        #region FilteredSelectedManufacturerBricks Tests

        [Fact]
        public void FilteredSelectedManufacturerBricks_ShouldReturnAllBricks_WhenSearchTermIsEmpty()
        {
            // Arrange
            var bricks = new List<IBrick>
            {
                MockDataFactory.Bricks.CreateValid(1, 1, "Red Brick"),
                MockDataFactory.Bricks.CreateValid(2, 1, "Blue Brick"),
                MockDataFactory.Bricks.CreateValid(3, 1, "Yellow Brick")
            };
            _viewModel.SelectedManufacturerBricks = bricks;
            _viewModel.ManufacturerBricksSearchTerm = string.Empty;

            // Act
            var result = _viewModel.FilteredSelectedManufacturerBricks;

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public void FilteredSelectedManufacturerBricks_ShouldFilterByName()
        {
            // Arrange
            var bricks = new List<IBrick>
            {
                MockDataFactory.Bricks.CreateValid(1, 1, "Red Brick"),
                MockDataFactory.Bricks.CreateValid(2, 1, "Blue Brick"),
                MockDataFactory.Bricks.CreateValid(3, 1, "Yellow Brick")
            };
            _viewModel.SelectedManufacturerBricks = bricks;
            _viewModel.ManufacturerBricksSearchTerm = "Red";

            // Act
            var result = _viewModel.FilteredSelectedManufacturerBricks;

            // Assert
            result.Should().HaveCount(1);
            result.First().Name.Should().Contain("Red");
        }

        #endregion

        #region OpenAddModal Tests

        [Fact]
        public void OpenAddModal_ShouldShowModalWithCleanState()
        {
            // Act
            _viewModel.OpenAddModal();

            // Assert
            _viewModel.ShowAddModal.Should().BeTrue();
            _viewModel.NewManufacturerModel.Should().NotBeNull();
            _viewModel.ValidationErrors.Should().BeEmpty();
            _viewModel.AddNameValidationMessage.Should().BeNull();
        }

        #endregion

        #region CloseAddModal Tests

        [Fact]
        public void CloseAddModal_ShouldHideModal()
        {
            // Arrange
            _viewModel.OpenAddModal();

            // Act
            _viewModel.CloseAddModal();

            // Assert
            _viewModel.ShowAddModal.Should().BeFalse();
            _viewModel.ValidationErrors.Should().BeEmpty();
            _viewModel.AddNameValidationMessage.Should().BeNull();
        }

        #endregion

        #region AddManufacturer Tests - Happy Path

        [Fact]
        public async Task AddManufacturer_ShouldAddManufacturer_WhenValidDataProvided()
        {
            // Arrange
            var newManufacturer = MockDataFactory.Manufacturers.CreateValidNew("NewManufacturer");
            _viewModel.NewManufacturerModel = newManufacturer;

            var createdManufacturer = MockDataFactory.Manufacturers.CreateValid(1, "NewManufacturer");
            _mockManufacturersMediator.Setup(m => m.Insert(It.IsAny<NewManufacturer>())).ReturnsAsync(createdManufacturer);
            // GetAllManufacturers should return empty list before insert, then return the created manufacturer after
            _mockManufacturersMediator.SetupSequence(m => m.GetAllManufacturers())
                .ReturnsAsync(Enumerable.Empty<IManufacturer>())
                .ReturnsAsync([createdManufacturer]);
            _mockBricksMediator.Setup(m => m.GetBricksByManufacturerId(1)).ReturnsAsync(Enumerable.Empty<IBrick>());

            // Act
            var result = await _viewModel.AddManufacturer();

            // Assert
            result.Should().BeTrue();
            _viewModel.ShowAddModal.Should().BeFalse();
            _viewModel.ValidationErrors.Should().BeEmpty();
            _mockManufacturersMediator.Verify(m => m.Insert(It.IsAny<NewManufacturer>()), Times.Once);
        }

        #endregion

        #region AddManufacturer Tests - Validation

        [Fact]
        public async Task AddManufacturer_ShouldReturnFalse_WhenNameAlreadyExists()
        {
            // Arrange
            var existingManufacturer = MockDataFactory.Manufacturers.CreateValid(1, "ExistingName");
            var newManufacturer = new NewManufacturer { Name = "ExistingName", Address1 = "123 Street", Postcode = "12345", PhoneNo = "+1234567890", Email = "test@example.com" };
            _viewModel.NewManufacturerModel = newManufacturer;
            _viewModel.Manufacturers = new List<IManufacturer> { existingManufacturer };

            // Mock GetAllManufacturers to return the existing manufacturer for duplicate name validation
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers())
                .ReturnsAsync(new List<IManufacturer> { existingManufacturer });

            // Act
            var result = await _viewModel.AddManufacturer();

            // Assert
            result.Should().BeFalse();
            _viewModel.ValidationErrors.Should().Contain("Name already exists. Please use a different manufacturer name.");
            _viewModel.AddNameValidationMessage.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region OpenEditModal Tests

        [Fact]
        public void OpenEditModal_ShouldShowModalWithManufacturerData()
        {
            // Arrange
            var manufacturer = MockDataFactory.Manufacturers.CreateValid(1, "Acme");

            // Act
            _viewModel.OpenEditModal(manufacturer);

            // Assert
            _viewModel.ShowEditModal.Should().BeTrue();
            _viewModel.EditManufacturerModel.Should().NotBeNull();
            _viewModel.EditManufacturerModel?.Id.Should().Be(1);
            _viewModel.ValidationErrors.Should().BeEmpty();
            _viewModel.EditNameValidationMessage.Should().BeNull();
        }

        #endregion

        #region CloseEditModal Tests

        [Fact]
        public void CloseEditModal_ShouldHideModal()
        {
            // Arrange
            var manufacturer = MockDataFactory.Manufacturers.CreateValid(1, "Acme");
            _viewModel.OpenEditModal(manufacturer);

            // Act
            _viewModel.CloseEditModal();

            // Assert
            _viewModel.ShowEditModal.Should().BeFalse();
            _viewModel.ValidationErrors.Should().BeEmpty();
            _viewModel.EditNameValidationMessage.Should().BeNull();
        }

        #endregion

        #region UpdateManufacturer Tests - Happy Path

        [Fact]
        public async Task UpdateManufacturer_ShouldUpdateManufacturer_WhenValidDataProvided()
        {
            // Arrange
            var existingManufacturer = MockDataFactory.Manufacturers.CreateValid(1, "OldName");
            var editManufacturer = new EditManufacturer(existingManufacturer);
            _viewModel.EditManufacturerModel = editManufacturer;
            _viewModel.Manufacturers = new List<IManufacturer> { existingManufacturer };

            _mockManufacturersMediator.Setup(m => m.Update(editManufacturer)).ReturnsAsync(existingManufacturer);
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(new List<IManufacturer> { existingManufacturer });
            _mockBricksMediator.Setup(m => m.GetBricksByManufacturerId(1)).ReturnsAsync(Enumerable.Empty<IBrick>());

            // Act
            var result = await _viewModel.UpdateManufacturer();

            // Assert
            result.Should().BeTrue();
            _viewModel.ShowEditModal.Should().BeFalse();
            _mockManufacturersMediator.Verify(m => m.Update(editManufacturer), Times.Once);
        }

        [Fact]
        public async Task UpdateManufacturer_ShouldReturnFalse_WhenEditModelIsNull()
        {
            // Arrange
            _viewModel.EditManufacturerModel = null;

            // Act
            var result = await _viewModel.UpdateManufacturer();

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region OpenDeleteConfirm Tests

        [Fact]
        public async Task OpenDeleteConfirm_ShouldShowModal_WhenNoBricksHaveActiveOrders()
        {
            // Arrange
            var manufacturer = MockDataFactory.Manufacturers.CreateValid(1, "Acme");
            var bricks = new List<IBrick>
            {
                MockDataFactory.Bricks.CreateValid(1, 1, "Brick 1"),
                MockDataFactory.Bricks.CreateValid(2, 1, "Brick 2")
            };
            _mockBricksMediator.Setup(m => m.GetBricksByManufacturerId(1)).ReturnsAsync(bricks);
            _mockOrdersMediator.Setup(m => m.GetBrickOrdersByBrickId(It.IsAny<long>())).ReturnsAsync(Enumerable.Empty<IBrickOrder>());

            // Act
            await _viewModel.OpenDeleteConfirm(manufacturer);

            // Assert
            _viewModel.ShowDeleteConfirm.Should().BeTrue();
            _viewModel.SelectedManufacturer.Should().Be(manufacturer);
            _viewModel.DeletionBlockedBricks.Should().BeEmpty();
            _viewModel.ChildBrickNames.Should().HaveCount(2);
        }

        [Fact]
        public async Task OpenDeleteConfirm_ShouldIdentifyBlockedBricks_WhenBricksHaveActiveOrders()
        {
            // Arrange
            var manufacturer = MockDataFactory.Manufacturers.CreateValid(1, "Acme");
            var bricks = new List<IBrick>
            {
                MockDataFactory.Bricks.CreateValid(1, 1, "Brick 1"),
                MockDataFactory.Bricks.CreateValid(2, 1, "Brick 2")
            };
            var activeOrder = MockDataFactory.BrickOrders.CreateValid(1, 1, isCancelled: false);
            var cancelledOrder = MockDataFactory.BrickOrders.CreateValid(2, 2, isCancelled: true);

            _mockBricksMediator.Setup(m => m.GetBricksByManufacturerId(1)).ReturnsAsync(bricks);
            _mockOrdersMediator.Setup(m => m.GetBrickOrdersByBrickId(1)).ReturnsAsync(new List<IBrickOrder> { activeOrder });
            _mockOrdersMediator.Setup(m => m.GetBrickOrdersByBrickId(2)).ReturnsAsync(new List<IBrickOrder> { cancelledOrder });

            // Act
            await _viewModel.OpenDeleteConfirm(manufacturer);

            // Assert
            _viewModel.DeletionBlockedBricks.Should().Contain("Brick 1");
            _viewModel.DeletionBlockedBricks.Should().NotContain("Brick 2");
        }

        #endregion

        #region CloseDeleteConfirm Tests

        [Fact]
        public void CloseDeleteConfirm_ShouldHideModal()
        {
            // Arrange
            _viewModel.ShowDeleteConfirm = true;
            _viewModel.ErrorMessage = "Some error";

            // Act
            _viewModel.CloseDeleteConfirm();

            // Assert
            _viewModel.ShowDeleteConfirm.Should().BeFalse();
            _viewModel.ErrorMessage.Should().BeNull();
        }

        #endregion

        #region CurrentUserRole Tests

        [Fact]
        public void CurrentUserRole_ShouldReturnAdminRole_WhenCurrentUserIsAdmin()
        {
            // Arrange
            var adminUser = MockDataFactory.Users.CreateValid(1, "admin", isAdmin: true);
            _mockAuthService.Setup(a => a.CurrentUser).Returns(adminUser);
            var viewModel = new ManufacturersViewModel(_mockManufacturersMediator.Object, _mockBricksMediator.Object, _mockOrdersMediator.Object, _mockAuthService.Object);

            // Act
            var role = viewModel.CurrentUserRole;

            // Assert
            role.Should().Be(UserRole.Admin);
        }

        #endregion

        #region Property Initialization Tests

        [Fact]
        public void OrdersViewModel_ShouldInitializeWithEmptyCollections()
        {
            // Act & Assert
            _viewModel.Manufacturers.Should().BeEmpty();
            _viewModel.SelectedManufacturerBricks.Should().BeEmpty();
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
            _viewModel.ShowAddModal.Should().BeFalse();
            _viewModel.ShowEditModal.Should().BeFalse();
            _viewModel.ShowDeleteConfirm.Should().BeFalse();
        }

        [Fact]
        public void OrdersViewModel_ShouldInitializeWithEmptySearchTerms()
        {
            // Act & Assert
            _viewModel.ManufacturersSearchTerm.Should().Be(string.Empty);
            _viewModel.ManufacturerBricksSearchTerm.Should().Be(string.Empty);
        }

        #endregion
    }
}
