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
    /// Comprehensive tests for BricksViewModel covering CRUD operations, validation, edge cases, and security
    /// </summary>
    public class BricksViewModelTests
    {
        private readonly Mock<IBricksMediator> _mockBricksMediator;
        private readonly Mock<IManufacturersMediator> _mockManufacturersMediator;
        private readonly Mock<IBrickOrdersMediator> _mockOrdersMediator;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly BricksViewModel _viewModel;

        public BricksViewModelTests()
        {
            _mockBricksMediator = new Mock<IBricksMediator>();
            _mockManufacturersMediator = new Mock<IManufacturersMediator>();
            _mockOrdersMediator = new Mock<IBrickOrdersMediator>();
            _mockAuthService = new Mock<IAuthService>();

            _viewModel = new BricksViewModel(
                _mockBricksMediator.Object,
                _mockManufacturersMediator.Object,
                _mockOrdersMediator.Object,
                _mockAuthService.Object
            );
        }

        #region LoadBricks Tests

        [Fact]
        public async Task LoadBricks_ShouldLoadBricksAndManufacturers_WhenCalled()
        {
            // Arrange
            var bricks = new List<IBrick>
            {
                MockDataFactory.Bricks.CreateValid(1),
                MockDataFactory.Bricks.CreateValid(2)
            };
            var manufacturers = new List<IManufacturer>
            {
                MockDataFactory.Manufacturers.CreateValid(1)
            };

            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(bricks);
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(manufacturers);

            // Act
            await _viewModel.LoadBricks();

            // Assert
            _viewModel.Bricks.Should().HaveCount(2);
            _viewModel.Manufacturers.Should().HaveCount(1);
            _viewModel.IsLoading.Should().BeFalse();
            _mockBricksMediator.Verify(m => m.GetAllBricks(), Times.Once);
            _mockManufacturersMediator.Verify(m => m.GetAllManufacturers(), Times.Once);
        }

        [Fact]
        public async Task LoadBricks_ShouldSetIsLoadingCorrectly()
        {
            // Arrange
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(Enumerable.Empty<IBrick>());
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(Enumerable.Empty<IManufacturer>());

            // Act
            await _viewModel.LoadBricks();

            // Assert
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task LoadBricks_ShouldHandleEmptyCollections()
        {
            // Arrange
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(Enumerable.Empty<IBrick>());
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(Enumerable.Empty<IManufacturer>());

            // Act
            await _viewModel.LoadBricks();

            // Assert
            _viewModel.Bricks.Should().BeEmpty();
            _viewModel.Manufacturers.Should().BeEmpty();
        }

        #endregion

        #region AddBrick Tests - Happy Path

        [Fact]
        public async Task AddBrick_ShouldSucceed_WhenAllDataIsValid()
        {
            // Arrange
            var newBrick = MockDataFactory.Bricks.CreateValidNew();
            var createdBrick = MockDataFactory.Bricks.CreateValid(1, newBrick.ManufacturerId, newBrick.Name);
            _viewModel.NewBrickModel = newBrick;

            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(new List<IBrick> { createdBrick });
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(Enumerable.Empty<IManufacturer>());
            _mockBricksMediator.Setup(m => m.Insert(It.IsAny<NewBrick>())).ReturnsAsync(createdBrick);
            _mockBricksMediator.Setup(m => m.GetBrickById(createdBrick.Id)).ReturnsAsync(createdBrick);
            _mockOrdersMediator.Setup(m => m.GetBrickOrdersByBrickId(createdBrick.Id)).ReturnsAsync(Enumerable.Empty<IBrickOrder>());

            // Act
            var result = await _viewModel.AddBrick();

            // Assert
            result.Should().BeTrue();
            _viewModel.ShowAddModal.Should().BeFalse();
            _viewModel.ValidationErrors.Should().BeEmpty();
            _mockBricksMediator.Verify(m => m.Insert(It.IsAny<NewBrick>()), Times.Once);
        }

        [Fact]
        public async Task AddBrick_ShouldHandleMinimumValidValues()
        {
            // Arrange
            var newBrick = MockDataFactory.Bricks.CreateValidNewWithMinimumValues();
            newBrick.Colour = "Red";
            newBrick.Material = "Clay";
            newBrick.Type = "Standard";
            var createdBrick = MockDataFactory.Bricks.CreateValid(1);
            _viewModel.NewBrickModel = newBrick;

            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(new List<IBrick> { createdBrick });
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(Enumerable.Empty<IManufacturer>());
            _mockBricksMediator.Setup(m => m.Insert(It.IsAny<NewBrick>())).ReturnsAsync(createdBrick);
            _mockBricksMediator.Setup(m => m.GetBrickById(createdBrick.Id)).ReturnsAsync(createdBrick);
            _mockOrdersMediator.Setup(m => m.GetBrickOrdersByBrickId(createdBrick.Id)).ReturnsAsync(Enumerable.Empty<IBrickOrder>());

            // Act
            var result = await _viewModel.AddBrick();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task AddBrick_ShouldHandleMaximumValidValues()
        {
            // Arrange
            var newBrick = MockDataFactory.Bricks.CreateValidNewWithMaximumValues();
            var createdBrick = MockDataFactory.Bricks.CreateValid(1);
            _viewModel.NewBrickModel = newBrick;

            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(new List<IBrick> { createdBrick });
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(Enumerable.Empty<IManufacturer>());
            _mockBricksMediator.Setup(m => m.Insert(It.IsAny<NewBrick>())).ReturnsAsync(createdBrick);
            _mockBricksMediator.Setup(m => m.GetBrickById(createdBrick.Id)).ReturnsAsync(createdBrick);
            _mockOrdersMediator.Setup(m => m.GetBrickOrdersByBrickId(createdBrick.Id)).ReturnsAsync(Enumerable.Empty<IBrickOrder>());

            // Act
            var result = await _viewModel.AddBrick();

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region AddBrick Tests - Validation Failures

        [Fact]
        public async Task AddBrick_ShouldFail_WhenNameIsEmpty()
        {
            // Arrange
            var newBrick = MockDataFactory.Bricks.CreateValidNew();
            newBrick.Name = string.Empty;
            _viewModel.NewBrickModel = newBrick;

            // Act
            var result = await _viewModel.AddBrick();

            // Assert
            result.Should().BeFalse();
            _viewModel.ValidationErrors.Should().NotBeEmpty();
            _viewModel.ShowAddModal.Should().BeTrue();
            _mockBricksMediator.Verify(m => m.Insert(It.IsAny<NewBrick>()), Times.Never);
        }

        [Fact]
        public async Task AddBrick_ShouldFail_WhenManufacturerIdIsZero()
        {
            // Arrange
            var newBrick = MockDataFactory.Bricks.CreateValidNew();
            newBrick.ManufacturerId = 0;
            _viewModel.NewBrickModel = newBrick;

            // Act
            var result = await _viewModel.AddBrick();

            // Assert
            result.Should().BeFalse();
            _viewModel.ValidationErrors.Should().Contain(e => e.Contains("Manufacturer"));
        }

        [Fact]
        public async Task AddBrick_ShouldFail_WhenPriceIsNegative()
        {
            // Arrange
            var newBrick = MockDataFactory.Bricks.CreateValidNew();
            newBrick.Price = -10m;
            _viewModel.NewBrickModel = newBrick;

            // Act
            var result = await _viewModel.AddBrick();

            // Assert
            result.Should().BeFalse();
            _viewModel.ValidationErrors.Should().Contain(e => e.Contains("Price") && e.Contains("negative"));
        }

        [Fact]
        public async Task AddBrick_ShouldFail_WhenDimensionsExceedMaximum()
        {
            // Arrange
            var newBrick = MockDataFactory.Bricks.CreateValidNew();
            newBrick.Width = 10001m;
            _viewModel.NewBrickModel = newBrick;

            // Act
            var result = await _viewModel.AddBrick();

            // Assert
            result.Should().BeFalse();
            _viewModel.ValidationErrors.Should().Contain(e => e.Contains("Dimensions") || e.Contains("10,000"));
        }

        [Fact]
        public async Task AddBrick_ShouldFail_WhenNameAlreadyExists()
        {
            // Arrange
            var existingBrick = MockDataFactory.Bricks.CreateValid(1, 1, "Existing Brick");
            var newBrick = MockDataFactory.Bricks.CreateValidNew();
            newBrick.Name = "Existing Brick";
            _viewModel.NewBrickModel = newBrick;

            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(new List<IBrick> { existingBrick });
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(Enumerable.Empty<IManufacturer>());

            // Act
            var result = await _viewModel.AddBrick();

            // Assert
            result.Should().BeFalse();
            _viewModel.HasAddBrickNameConflict.Should().BeTrue();
        }

        #endregion

        #region UpdateBrick Tests

        [Fact]
        public async Task UpdateBrick_ShouldSucceed_WhenAllDataIsValid()
        {
            // Arrange
            var existingBrick = MockDataFactory.Bricks.CreateValid(1);
            var editBrick = new EditBrick(existingBrick);
            editBrick.Name = "Updated Name";
            _viewModel.EditBrickModel = editBrick;

            _mockBricksMediator.Setup(m => m.Update(It.IsAny<EditBrick>())).ReturnsAsync(existingBrick);
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(new List<IBrick> { existingBrick });
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(Enumerable.Empty<IManufacturer>());
            _mockBricksMediator.Setup(m => m.GetBrickById(existingBrick.Id)).ReturnsAsync(existingBrick);
            _mockOrdersMediator.Setup(m => m.GetBrickOrdersByBrickId(existingBrick.Id)).ReturnsAsync(Enumerable.Empty<IBrickOrder>());

            // Act
            var result = await _viewModel.UpdateBrick();

            // Assert
            result.Should().BeTrue();
            _viewModel.ShowEditModal.Should().BeFalse();
            _viewModel.ValidationErrors.Should().BeEmpty();
            _mockBricksMediator.Verify(m => m.Update(It.IsAny<EditBrick>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBrick_ShouldReturnFalse_WhenEditBrickModelIsNull()
        {
            // Arrange
            _viewModel.EditBrickModel = null;

            // Act
            var result = await _viewModel.UpdateBrick();

            // Assert
            result.Should().BeFalse();
            _mockBricksMediator.Verify(m => m.Update(It.IsAny<EditBrick>()), Times.Never);
        }

        #endregion

        #region DeleteBrick Tests

        [Fact]
        public async Task DeleteBrick_ShouldSucceed_WhenNoOrdersExist()
        {
            // Arrange
            var brick = MockDataFactory.Bricks.CreateValid(1);
            _viewModel.SelectedBrick = brick;

            _mockOrdersMediator.Setup(m => m.GetBrickOrdersByBrickId(brick.Id)).ReturnsAsync(Enumerable.Empty<IBrickOrder>());
            _mockBricksMediator.Setup(m => m.Delete(brick.Id)).Returns(Task.CompletedTask);
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(Enumerable.Empty<IBrick>());
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(Enumerable.Empty<IManufacturer>());

            // Act
            var result = await _viewModel.DeleteBrick();

            // Assert
            result.Should().BeTrue();
            _viewModel.SelectedBrick.Should().BeNull();
            _viewModel.ShowDeleteConfirm.Should().BeFalse();
            _mockBricksMediator.Verify(m => m.Delete(brick.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteBrick_ShouldFail_WhenOrdersExist()
        {
            // Arrange
            var brick = MockDataFactory.Bricks.CreateValid(1);
            var orders = new List<IBrickOrder>
            {
                MockDataFactory.BrickOrders.CreateValid(1, brick.Id)
            };
            _viewModel.SelectedBrick = brick;

            _mockOrdersMediator.Setup(m => m.GetBrickOrdersByBrickId(brick.Id)).ReturnsAsync(orders);

            // Act
            var result = await _viewModel.DeleteBrick();

            // Assert
            result.Should().BeFalse();
            _viewModel.ErrorMessage.Should().Contain("Cannot delete");
            _mockBricksMediator.Verify(m => m.Delete(It.IsAny<long>()), Times.Never);
        }

        [Fact]
        public async Task DeleteBrick_ShouldReturnFalse_WhenSelectedBrickIsNull()
        {
            // Arrange
            _viewModel.SelectedBrick = null;

            // Act
            var result = await _viewModel.DeleteBrick();

            // Assert
            result.Should().BeFalse();
            _mockBricksMediator.Verify(m => m.Delete(It.IsAny<long>()), Times.Never);
        }

        #endregion

        #region Numeric Input Validation Tests

        [Theory]
        [InlineData("0", 0)]
        [InlineData("10.5", 10.5)]
        [InlineData("999.99", 999.99)]
        [InlineData("-5", -5)]
        [InlineData("", 0)]
        [InlineData(null, 0)]
        [InlineData("invalid", 0)]
        public void ParseDecimalInput_ShouldHandleVariousInputs(string? input, decimal expected)
        {
            // Act
            var result = BricksViewModel.ParseDecimalInput(input);

            // Assert
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData("0", 0)]
        [InlineData("50", 0.5)]
        [InlineData("100", 1)]
        [InlineData("150", 1)]  // Clamped to max
        [InlineData("-10", 0)]  // Clamped to min
        public void ParseVoidsPercentInput_ShouldClampValues(string input, decimal expected)
        {
            // Act
            var result = BricksViewModel.ParseVoidsPercentInput(input);

            // Assert
            result.Should().Be(expected);
        }

        #endregion

        #region Filter Tests

        [Fact]
        public void FilteredBricks_ShouldReturnAllBricks_WhenSearchTermIsEmpty()
        {
            // Arrange
            _viewModel.BricksSearchTerm = string.Empty;
            var brick1 = MockDataFactory.Bricks.CreateValid(1, 1, "Brick A");
            var brick2 = MockDataFactory.Bricks.CreateValid(2, 1, "Brick B");
            _viewModel.Bricks = new List<IBrick> { brick1, brick2 };

            // Act
            var filtered = _viewModel.FilteredBricks.ToList();

            // Assert
            filtered.Should().HaveCount(2);
        }

        [Fact]
        public void FilteredBricks_ShouldFilterByName()
        {
            // Arrange
            var brick1 = MockDataFactory.Bricks.CreateValid(1, 1, "Red Brick");
            var brick2 = MockDataFactory.Bricks.CreateValid(2, 1, "Blue Brick");
            _viewModel.Bricks = new List<IBrick> { brick1, brick2 };
            _viewModel.BricksSearchTerm = "Red";

            // Act
            var filtered = _viewModel.FilteredBricks.ToList();

            // Assert
            filtered.Should().HaveCount(1);
            filtered.First().Name.Should().Be("Red Brick");
        }

        [Fact]
        public void FilteredBricks_ShouldBeCaseInsensitive()
        {
            // Arrange
            var brick = MockDataFactory.Bricks.CreateValid(1, 1, "Red Brick");
            _viewModel.Bricks = new List<IBrick> { brick };
            _viewModel.BricksSearchTerm = "red";

            // Act
            var filtered = _viewModel.FilteredBricks.ToList();

            // Assert
            filtered.Should().HaveCount(1);
        }

        #endregion

        #region Modal Tests

        [Fact]
        public void OpenAddModal_ShouldResetState()
        {
            // Arrange
            _viewModel.ValidationErrors.Add("Some error");

            // Act
            _viewModel.OpenAddModal();

            // Assert
            _viewModel.ShowAddModal.Should().BeTrue();
            _viewModel.ValidationErrors.Should().BeEmpty();
            _viewModel.NewBrickModel.Should().NotBeNull();
        }

        [Fact]
        public void CloseAddModal_ShouldResetState()
        {
            // Act
            _viewModel.CloseAddModal();

            // Assert
            _viewModel.ShowAddModal.Should().BeFalse();
            _viewModel.ValidationErrors.Should().BeEmpty();
        }

        [Fact]
        public void OpenEditModal_ShouldSetEditBrickModel()
        {
            // Arrange
            var brick = MockDataFactory.Bricks.CreateValid(1);
            var manufacturer = MockDataFactory.Manufacturers.CreateValid(1);
            _viewModel.Manufacturers = new List<IManufacturer> { manufacturer };

            // Act
            _viewModel.OpenEditModal(brick);

            // Assert
            _viewModel.ShowEditModal.Should().BeTrue();
            _viewModel.EditBrickModel.Should().NotBeNull();
            _viewModel.EditBrickModel!.Id.Should().Be(brick.Id);
        }

        #endregion

        #region Role Tests

        [Fact]
        public void CurrentUserRole_ShouldReturnStandard_WhenNotAuthenticated()
        {
            // Arrange
            _mockAuthService.Setup(a => a.CurrentUser).Returns((SoftwareEngineeringDevOps.App.Users.IUserInfo?)null);

            // Act
            var role = _viewModel.CurrentUserRole;

            // Assert
            role.Should().Be(UserRole.Standard);
        }

        [Fact]
        public void CurrentUserRole_ShouldReturnAdmin_WhenUserIsAdmin()
        {
            // Arrange
            var adminUser = MockDataFactory.Users.CreateAdmin();
            _mockAuthService.Setup(a => a.CurrentUser).Returns(adminUser);

            // Act
            var role = _viewModel.CurrentUserRole;

            // Assert
            role.Should().Be(UserRole.Admin);
        }

        #endregion
    }
}
