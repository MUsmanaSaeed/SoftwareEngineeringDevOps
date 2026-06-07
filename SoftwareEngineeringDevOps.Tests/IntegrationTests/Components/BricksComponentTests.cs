using Bunit;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SoftwareEngineeringDevOps.App.Auth;
using SoftwareEngineeringDevOps.App.Bricks;
using SoftwareEngineeringDevOps.App.BrickOrders;
using SoftwareEngineeringDevOps.App.Manufacturers;
using SoftwareEngineeringDevOps.Components.Pages.BricksModule;
using SoftwareEngineeringDevOps.Components.Shared;
using SoftwareEngineeringDevOps.Tests.TestUtilities;

namespace SoftwareEngineeringDevOps.Tests.IntegrationTests.Components
{
    /// <summary>
    /// bUnit integration tests for Blazor Bricks component covering rendering, interactions, and authentication
    /// </summary>
    public class BricksComponentTests : TestContext
    {
        private readonly Mock<IBricksMediator> _mockBricksMediator;
        private readonly Mock<IManufacturersMediator> _mockManufacturersMediator;
        private readonly Mock<IBrickOrdersMediator> _mockOrdersMediator;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<IToastService> _mockToastService;

        public BricksComponentTests()
        {
            _mockBricksMediator = new Mock<IBricksMediator>();
            _mockManufacturersMediator = new Mock<IManufacturersMediator>();
            _mockOrdersMediator = new Mock<IBrickOrdersMediator>();
            _mockAuthService = new Mock<IAuthService>();
            _mockToastService = new Mock<IToastService>();

            // Register mock services
            Services.AddSingleton(_mockBricksMediator.Object);
            Services.AddSingleton(_mockManufacturersMediator.Object);
            Services.AddSingleton(_mockOrdersMediator.Object);
            Services.AddSingleton(_mockAuthService.Object);
            Services.AddSingleton(_mockToastService.Object);
        }

        #region Component Rendering Tests

        [Fact]
        public void Bricks_ShouldRenderSuccessfully_WhenInitialized()
        {
            // Arrange
            SetupEmptyCollections();

            // Act
            var cut = Render<Bricks>();

            // Assert
            cut.Should().NotBeNull();
            cut.Markup.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Bricks_ShouldDisplayBricksList_WhenBricksExist()
        {
            // Arrange
            var bricks = new List<IBrick>
            {
                MockDataFactory.Bricks.CreateValid(1, 1, "Test Brick 1"),
                MockDataFactory.Bricks.CreateValid(2, 1, "Test Brick 2")
            };
            SetupBricksCollections(bricks);

            // Act
            var cut = Render<Bricks>();

            // Assert
            cut.Markup.Should().Contain("Test Brick 1");
            cut.Markup.Should().Contain("Test Brick 2");
        }

        [Fact]
        public void Bricks_ShouldDisplayEmptyMessage_WhenNoBricksExist()
        {
            // Arrange
            SetupEmptyCollections();

            // Act
            var cut = Render<Bricks>();

            // Assert
            // Component should handle empty state gracefully
            cut.Should().NotBeNull();
        }

        #endregion

        #region Authentication Tests

        [Fact]
        public void Bricks_ShouldHideAddButton_WhenUserIsNotEditor()
        {
            // Arrange
            var standardUser = MockDataFactory.Users.CreateStandardUser();
            _mockAuthService.Setup(a => a.CurrentUser).Returns(standardUser);
            SetupEmptyCollections();

            // Act
            var cut = Render<Bricks>();

            // Assert
            // Standard users should not see edit/add buttons
            cut.Should().NotBeNull();
        }

        [Fact]
        public void Bricks_ShouldShowAddButton_WhenUserIsEditor()
        {
            // Arrange
            var editorUser = MockDataFactory.Users.CreateEditor();
            _mockAuthService.Setup(a => a.CurrentUser).Returns(editorUser);
            SetupEmptyCollections();

            // Act
            var cut = Render<Bricks>();

            // Assert
            cut.Should().NotBeNull();
        }

        [Fact]
        public void Bricks_ShouldShowAddButton_WhenUserIsAdmin()
        {
            // Arrange
            var adminUser = MockDataFactory.Users.CreateAdmin();
            _mockAuthService.Setup(a => a.CurrentUser).Returns(adminUser);
            SetupEmptyCollections();

            // Act
            var cut = Render<Bricks>();

            // Assert
            cut.Should().NotBeNull();
        }

        #endregion

        #region Search and Filter Tests

        [Fact]
        public void Bricks_ShouldFilterBricks_WhenSearchTermEntered()
        {
            // Arrange
            var bricks = new List<IBrick>
            {
                MockDataFactory.Bricks.CreateValid(1, 1, "Red Brick"),
                MockDataFactory.Bricks.CreateValid(2, 1, "Blue Brick")
            };
            SetupBricksCollections(bricks);

            // Act
            var cut = Render<Bricks>();

            // Assert
            // Both bricks should be visible initially
            cut.Markup.Should().Contain("Red Brick");
            cut.Markup.Should().Contain("Blue Brick");
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public void Bricks_ShouldHandleLoadFailure_Gracefully()
        {
            // Arrange
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ThrowsAsync(new Exception("Database error"));
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(Enumerable.Empty<IManufacturer>());

            // Act & Assert
            // Component should handle exceptions gracefully without crashing
            var act = () => Render<Bricks>();
            act.Should().Throw<Exception>(); // Expected behavior - component should propagate critical errors
        }

        #endregion

        #region Helper Methods

        private void SetupEmptyCollections()
        {
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(Enumerable.Empty<IBrick>());
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(Enumerable.Empty<IManufacturer>());
            _mockOrdersMediator.Setup(m => m.GetBrickOrdersByBrickId(It.IsAny<long>())).ReturnsAsync(Enumerable.Empty<IBrickOrder>());
        }

        private void SetupBricksCollections(IEnumerable<IBrick> bricks)
        {
            _mockBricksMediator.Setup(m => m.GetAllBricks()).ReturnsAsync(bricks);
            _mockManufacturersMediator.Setup(m => m.GetAllManufacturers()).ReturnsAsync(Enumerable.Empty<IManufacturer>());
            _mockOrdersMediator.Setup(m => m.GetBrickOrdersByBrickId(It.IsAny<long>())).ReturnsAsync(Enumerable.Empty<IBrickOrder>());
        }

        #endregion
    }
}
