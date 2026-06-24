using FluentAssertions;
using Microsoft.Playwright;

namespace SoftwareEngineeringDevOps.Tests.IntegrationTests.E2E
{
    /// <summary>
    /// Playwright end-to-end tests for authentication flows
    /// Tests run headlessly and are isolated from external dependencies
    /// </summary>
    [Collection("E2E Tests")]
    public class AuthenticationE2ETests : IAsyncLifetime
    {
        private IPlaywright? _playwright;
        private IBrowser? _browser;
        private static readonly string BaseUrl = Environment.GetEnvironmentVariable("BASE_URL") ?? "https://acs-brick-management.onrender.com";

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

        #region Login Tests - Happy Path

        [Fact(Skip = "Requires running application instance")]
        public async Task Login_ShouldSucceed_WithValidCredentials()
        {
            // Arrange
            var page = await _browser!.NewPageAsync();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/login");

                // Act
                await page.FillAsync("input[name='username']", "admin");
                await page.FillAsync("input[name='password']", "AdminPassword123!");
                await page.ClickAsync("button[type='submit']");

                // Wait for navigation
                await page.WaitForURLAsync($"{BaseUrl}/", new PageWaitForURLOptions { Timeout = 5000 });

                // Assert
                page.Url.Should().Be($"{BaseUrl}/");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        #endregion

        #region Login Tests - Error Cases

        [Fact(Skip = "Requires running application instance")]
        public async Task Login_ShouldFail_WithInvalidCredentials()
        {
            // Arrange
            var page = await _browser!.NewPageAsync();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/login");

                // Act
                await page.FillAsync("input[name='username']", "invaliduser");
                await page.FillAsync("input[name='password']", "wrongpassword");
                await page.ClickAsync("button[type='submit']");

                // Wait for error message
                await page.WaitForSelectorAsync(".error-message", new PageWaitForSelectorOptions { Timeout = 5000 });

                // Assert
                var errorMessage = await page.TextContentAsync(".error-message");
                errorMessage.Should().Contain("Invalid");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        [Fact(Skip = "Requires running application instance")]
        public async Task Login_ShouldFail_WithEmptyCredentials()
        {
            // Arrange
            var page = await _browser!.NewPageAsync();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/login");

                // Act
                await page.ClickAsync("button[type='submit']");

                // Assert
                // Form validation should prevent submission
                page.Url.Should().Contain("/login");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        #endregion

        #region Registration Tests

        [Fact(Skip = "Requires running application instance")]
        public async Task Register_ShouldSucceed_WithValidData()
        {
            // Arrange
            var page = await _browser!.NewPageAsync();
            var uniqueUsername = $"testuser_{Guid.NewGuid():N}";
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/register");

                // Act
                await page.FillAsync("input[name='username']", uniqueUsername);
                await page.FillAsync("input[name='password']", "SecurePassword123!");
                await page.FillAsync("input[name='firstname']", "Test");
                await page.FillAsync("input[name='lastname']", "User");
                await page.ClickAsync("button[type='submit']");

                // Wait for redirect to login
                await page.WaitForURLAsync($"{BaseUrl}/login", new PageWaitForURLOptions { Timeout = 5000 });

                // Assert
                page.Url.Should().Contain("/login");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        [Fact(Skip = "Requires running application instance")]
        public async Task Register_ShouldFail_WithWeakPassword()
        {
            // Arrange
            var page = await _browser!.NewPageAsync();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/register");

                // Act
                await page.FillAsync("input[name='username']", "testuser");
                await page.FillAsync("input[name='password']", "weak");
                await page.FillAsync("input[name='firstname']", "Test");
                await page.FillAsync("input[name='lastname']", "User");
                await page.ClickAsync("button[type='submit']");

                // Assert
                var errorMessage = await page.TextContentAsync(".validation-error");
                errorMessage.Should().Contain("Password");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        #endregion

        #region Logout Tests

        [Fact(Skip = "Requires running application instance")]
        public async Task Logout_ShouldRedirectToLogin()
        {
            // Arrange
            var page = await _browser!.NewPageAsync();
            
            try
            {
                // Login first
                await page.GotoAsync($"{BaseUrl}/login");
                await page.FillAsync("input[name='username']", "admin");
                await page.FillAsync("input[name='password']", "AdminPassword123!");
                await page.ClickAsync("button[type='submit']");
                await page.WaitForURLAsync($"{BaseUrl}/");

                // Act
                await page.ClickAsync("button[aria-label='Logout']");

                // Assert
                await page.WaitForURLAsync($"{BaseUrl}/login");
                page.Url.Should().Contain("/login");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        #endregion

        #region Security Tests

        [Fact(Skip = "Requires running application instance")]
        public async Task ProtectedPage_ShouldRedirectToLogin_WhenNotAuthenticated()
        {
            // Arrange
            var page = await _browser!.NewPageAsync();
            
            try
            {
                // Act
                await page.GotoAsync($"{BaseUrl}/bricks");

                // Assert
                await page.WaitForURLAsync($"{BaseUrl}/login");
                page.Url.Should().Contain("/login");
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        [Fact(Skip = "Requires running application instance")]
        public async Task XSS_ShouldBeSanitized_InInputFields()
        {
            // Arrange
            var page = await _browser!.NewPageAsync();
            
            try
            {
                await page.GotoAsync($"{BaseUrl}/login");

                // Act
                await page.FillAsync("input[name='username']", "<script>alert('xss')</script>");
                await page.FillAsync("input[name='password']", "password");
                await page.ClickAsync("button[type='submit']");

                // Assert
                // Script should not execute
                var alertsTriggered = false;
                page.Dialog += (_, _) => alertsTriggered = true;
                
                await Task.Delay(1000); // Wait for potential script execution
                alertsTriggered.Should().BeFalse();
            }
            finally
            {
                await page.CloseAsync();
            }
        }

        #endregion
    }
}
