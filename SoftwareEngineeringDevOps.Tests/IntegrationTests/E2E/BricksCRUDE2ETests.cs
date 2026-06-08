using FluentAssertions;
using Microsoft.Playwright;

namespace SoftwareEngineeringDevOps.Tests.IntegrationTests.E2E
{
    /// <summary>
    /// Playwright end-to-end tests for Bricks CRUD operations
    /// Covers happy paths, boundary conditions, and error handling
    /// </summary>
    [Collection("E2E Tests")]
    public class BricksCRUDE2ETests : IAsyncLifetime
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;
        private const string BaseUrl = "http://localhost:5000";

        public async ValueTask InitializeAsync()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = ["--no-sandbox", "--disable-setuid-sandbox"]
            });
        }

        public async ValueTask DisposeAsync()
        {
            if (_browser != null)
            {
                await _browser.CloseAsync();
                await _browser.DisposeAsync();
            }
            _playwright?.Dispose();
        }

        #region Helper Methods

        private async Task<IPage> LoginAsAdmin()
        {
            var page = await _browser!.NewPageAsync();
            await page.GotoAsync($"{BaseUrl}/login");
            await page.FillAsync("input[name='username']", "admin");
            await page.FillAsync("input[name='password']", "AdminPassword123!");
            await page.ClickAsync("button[type='submit']");
            await page.WaitForURLAsync($"{BaseUrl}/");
            return page;
        }

        #endregion

        #region Create Tests - Happy Path

        [Fact(Skip = "Requires running application instance")]
        public async Task CreateBrick_ShouldSucceed_WithValidData()
        {
            // Arrange
            var page = await LoginAsAdmin();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/bricks");

                // Act
                await page.ClickAsync("button[aria-label='Add Brick']");
                await page.FillAsync("input[name='name']", "E2E Test Brick");
                await page.SelectOptionAsync("select[name='manufacturer']", "1");
                await page.FillAsync("input[name='price']", "15.99");
                await page.FillAsync("input[name='colour']", "Red");
                await page.FillAsync("input[name='material']", "Clay");
                await page.FillAsync("input[name='type']", "Standard");
                await page.FillAsync("input[name='strength']", "25.5");
                await page.FillAsync("input[name='width']", "215");
                await page.FillAsync("input[name='height']", "102.5");
                await page.FillAsync("input[name='depth']", "65");
                await page.ClickAsync("button[type='submit']");

                // Wait for modal to close
                await page.WaitForSelectorAsync("button[aria-label='Add Brick']", new PageWaitForSelectorOptions { Timeout = 5000 });

                // Assert
                var content = await page.TextContentAsync("body");
                content.Should().Contain("E2E Test Brick");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        #endregion

        #region Create Tests - Validation

        [Fact(Skip = "Requires running application instance")]
        public async Task CreateBrick_ShouldFail_WithEmptyName()
        {
            // Arrange
            var page = await LoginAsAdmin();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/bricks");

                // Act
                await page.ClickAsync("button[aria-label='Add Brick']");
                await page.FillAsync("input[name='name']", "");
                await page.ClickAsync("button[type='submit']");

                // Assert
                var errorMessage = await page.TextContentAsync(".validation-error");
                errorMessage.Should().Contain("required");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        [Fact(Skip = "Requires running application instance")]
        public async Task CreateBrick_ShouldFail_WithNegativePrice()
        {
            // Arrange
            var page = await LoginAsAdmin();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/bricks");

                // Act
                await page.ClickAsync("button[aria-label='Add Brick']");
                await page.FillAsync("input[name='name']", "Invalid Brick");
                await page.FillAsync("input[name='price']", "-10");
                await page.ClickAsync("button[type='submit']");

                // Assert
                var errorMessage = await page.TextContentAsync(".validation-error");
                errorMessage.Should().Contain("negative");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        [Fact(Skip = "Requires running application instance")]
        public async Task CreateBrick_ShouldFail_WithExcessiveDimensions()
        {
            // Arrange
            var page = await LoginAsAdmin();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/bricks");

                // Act
                await page.ClickAsync("button[aria-label='Add Brick']");
                await page.FillAsync("input[name='name']", "Oversized Brick");
                await page.FillAsync("input[name='width']", "10001");
                await page.ClickAsync("button[type='submit']");

                // Assert
                var errorMessage = await page.TextContentAsync(".validation-error");
                errorMessage.Should().Contain("10,000");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        #endregion

        #region Read Tests

        [Fact(Skip = "Requires running application instance")]
        public async Task ViewBricks_ShouldDisplayList()
        {
            // Arrange
            var page = await LoginAsAdmin();
            
            try
            {
                // Act
                await page.GotoAsync($"{BaseUrl}/bricks");

                // Assert
                var heading = await page.TextContentAsync("h1");
                heading.Should().Contain("Bricks");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        [Fact(Skip = "Requires running application instance")]
        public async Task SearchBricks_ShouldFilterResults()
        {
            // Arrange
            var page = await LoginAsAdmin();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/bricks");

                // Act
                await page.FillAsync("input[name='search']", "Red");

                // Wait for filtering
                await Task.Delay(500);

                // Assert
                var visibleBricks = await page.QuerySelectorAllAsync(".brick-item:visible");
                visibleBricks.Should().NotBeEmpty();
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        #endregion

        #region Update Tests

        [Fact(Skip = "Requires running application instance")]
        public async Task UpdateBrick_ShouldSucceed_WithValidChanges()
        {
            // Arrange
            var page = await LoginAsAdmin();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/bricks");

                // Act
                await page.ClickAsync(".brick-item:first-child button[aria-label='Edit']");
                await page.FillAsync("input[name='price']", "20.99");
                await page.ClickAsync("button[type='submit']");

                // Wait for modal to close
                await page.WaitForSelectorAsync(".brick-item:first-child", new PageWaitForSelectorOptions { Timeout = 5000 });

                // Assert
                var content = await page.TextContentAsync(".brick-item:first-child");
                content.Should().Contain("20.99");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        #endregion

        #region Delete Tests

        [Fact(Skip = "Requires running application instance")]
        public async Task DeleteBrick_ShouldSucceed_WhenNoOrders()
        {
            // Arrange
            var page = await LoginAsAdmin();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/bricks");

                // Act
                var initialCount = await page.QuerySelectorAllAsync(".brick-item");
                await page.ClickAsync(".brick-item:first-child button[aria-label='Delete']");
                await page.ClickAsync("button[aria-label='Confirm Delete']");

                // Wait for deletion
                await Task.Delay(1000);

                // Assert
                var finalCount = await page.QuerySelectorAllAsync(".brick-item");
                finalCount.Count.Should().BeLessThan(initialCount.Count);
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        [Fact(Skip = "Requires running application instance")]
        public async Task DeleteBrick_ShouldFail_WhenOrdersExist()
        {
            // Arrange
            var page = await LoginAsAdmin();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/bricks");

                // Act
                await page.ClickAsync(".brick-item-with-orders button[aria-label='Delete']");

                // Assert
                var errorMessage = await page.TextContentAsync(".error-message");
                errorMessage.Should().Contain("Cannot delete");
                errorMessage.Should().Contain("order");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        #endregion

        #region Authorization Tests

        [Fact(Skip = "Requires running application instance")]
        public async Task CreateBrick_ShouldBeHidden_ForStandardUser()
        {
            // Arrange
            var page = await _browser!.NewPageAsync();
            await page.GotoAsync($"{BaseUrl}/login");
            await page.FillAsync("input[name='username']", "standarduser");
            await page.FillAsync("input[name='password']", "StandardPassword123!");
            await page.ClickAsync("button[type='submit']");
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/bricks");

                // Assert
                var addButton = await page.QuerySelectorAsync("button[aria-label='Add Brick']");
                addButton.Should().BeNull();
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        #endregion

        #region Boundary Tests

        [Fact(Skip = "Requires running application instance")]
        public async Task CreateBrick_ShouldAccept_MinimumValues()
        {
            // Arrange
            var page = await LoginAsAdmin();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/bricks");

                // Act
                await page.ClickAsync("button[aria-label='Add Brick']");
                await page.FillAsync("input[name='name']", "Min Brick");
                await page.SelectOptionAsync("select[name='manufacturer']", "1");
                await page.FillAsync("input[name='price']", "0");
                await page.FillAsync("input[name='colour']", "A");
                await page.FillAsync("input[name='material']", "B");
                await page.FillAsync("input[name='type']", "C");
                await page.FillAsync("input[name='strength']", "0");
                await page.FillAsync("input[name='width']", "0");
                await page.FillAsync("input[name='height']", "0");
                await page.FillAsync("input[name='depth']", "0");
                await page.ClickAsync("button[type='submit']");

                // Wait for modal to close
                await page.WaitForSelectorAsync("button[aria-label='Add Brick']");

                // Assert
                var content = await page.TextContentAsync("body");
                content.Should().Contain("Min Brick");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        [Fact(Skip = "Requires running application instance")]
        public async Task CreateBrick_ShouldAccept_MaximumDimensions()
        {
            // Arrange
            var page = await LoginAsAdmin();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/bricks");

                // Act
                await page.ClickAsync("button[aria-label='Add Brick']");
                await page.FillAsync("input[name='name']", "Max Brick");
                await page.SelectOptionAsync("select[name='manufacturer']", "1");
                await page.FillAsync("input[name='price']", "9999.99");
                await page.FillAsync("input[name='colour']", "Multi");
                await page.FillAsync("input[name='material']", "Concrete");
                await page.FillAsync("input[name='type']", "Engineering");
                await page.FillAsync("input[name='strength']", "999.99");
                await page.FillAsync("input[name='width']", "10000");
                await page.FillAsync("input[name='height']", "10000");
                await page.FillAsync("input[name='depth']", "10000");
                await page.ClickAsync("button[type='submit']");

                // Wait for modal to close
                await page.WaitForSelectorAsync("button[aria-label='Add Brick']");

                // Assert
                var content = await page.TextContentAsync("body");
                content.Should().Contain("Max Brick");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        #endregion

        #region Network Error Simulation

        [Fact(Skip = "Requires running application instance")]
        public async Task CreateBrick_ShouldHandleTimeout_Gracefully()
        {
            // Arrange
            var page = await LoginAsAdmin();
            
            try
            {
                // Simulate slow network
                await page.RouteAsync("**/api/**", async route =>
                {
                    await Task.Delay(30000); // 30 second delay
                    await route.AbortAsync();
                });

                await page.GotoAsync($"{BaseUrl}/bricks");

                // Act
                await page.ClickAsync("button[aria-label='Add Brick']");
                await page.FillAsync("input[name='name']", "Timeout Test");
                await page.ClickAsync("button[type='submit']");

                // Assert
                var errorMessage = await page.WaitForSelectorAsync(".error-message", new PageWaitForSelectorOptions { Timeout = 5000 });
                errorMessage.Should().NotBeNull();
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        #endregion
    }
}
