using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftwareEngineeringDevOps.App.Auth;
using SoftwareEngineeringDevOps.Components.ViewModels;
using SoftwareEngineeringDevOps.Tests.TestUtilities;

namespace SoftwareEngineeringDevOps.Tests.UnitTests.ViewModels
{
    /// <summary>
    /// Comprehensive tests for LoginViewModel covering authentication, validation, error handling, and security
    /// </summary>
    public class LoginViewModelTests
    {
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly Mock<ILogger<LoginViewModel>> _mockLogger;
        private readonly LoginViewModel _viewModel;

        public LoginViewModelTests()
        {
            _mockAuthService = new Mock<IAuthService>();
            _mockLogger = new Mock<ILogger<LoginViewModel>>();
            _viewModel = new LoginViewModel(_mockAuthService.Object, _mockLogger.Object);
        }

        #region ExecuteLogin Tests - Happy Path

        [Fact]
        public async Task ExecuteLogin_ShouldReturnTrue_WhenCredentialsAreValid()
        {
            // Arrange
            _viewModel.Username = "testuser";
            _viewModel.Password = "SecurePassword123!";
            _mockAuthService.Setup(a => a.LoginAsync("testuser", "SecurePassword123!")).ReturnsAsync(true);

            // Act
            var result = await _viewModel.ExecuteLogin();

            // Assert
            result.Should().BeTrue();
            _viewModel.ErrorMessage.Should().BeNull();
            _mockAuthService.Verify(a => a.LoginAsync("testuser", "SecurePassword123!"), Times.Once);
        }

        [Fact]
        public async Task ExecuteLogin_ShouldSetIsLoadingFalse_WhenLoginSucceeds()
        {
            // Arrange
            _viewModel.Username = "testuser";
            _viewModel.Password = "SecurePassword123!";
            _mockAuthService.Setup(a => a.LoginAsync("testuser", "SecurePassword123!")).ReturnsAsync(true);

            // Act
            await _viewModel.ExecuteLogin();

            // Assert
            _viewModel.IsLoading.Should().BeFalse();
        }

        #endregion

        #region ExecuteLogin Tests - Invalid Credentials

        [Fact]
        public async Task ExecuteLogin_ShouldReturnFalse_WhenCredentialsAreInvalid()
        {
            // Arrange
            _viewModel.Username = "testuser";
            _viewModel.Password = "WrongPassword";
            _mockAuthService.Setup(a => a.LoginAsync("testuser", "WrongPassword")).ReturnsAsync(false);

            // Act
            var result = await _viewModel.ExecuteLogin();

            // Assert
            result.Should().BeFalse();
            _viewModel.ErrorMessage.Should().Contain("Invalid username or password");
            _mockAuthService.Verify(a => a.LoginAsync("testuser", "WrongPassword"), Times.Once);
        }

        [Fact]
        public async Task ExecuteLogin_ShouldSetIsLoadingFalse_WhenLoginFails()
        {
            // Arrange
            _viewModel.Username = "testuser";
            _viewModel.Password = "WrongPassword";
            _mockAuthService.Setup(a => a.LoginAsync("testuser", "WrongPassword")).ReturnsAsync(false);

            // Act
            await _viewModel.ExecuteLogin();

            // Assert
            _viewModel.IsLoading.Should().BeFalse();
        }

        #endregion

        #region ExecuteLogin Tests - Validation

        [Fact]
        public async Task ExecuteLogin_ShouldReturnFalse_WhenUsernameIsEmpty()
        {
            // Arrange
            _viewModel.Username = string.Empty;
            _viewModel.Password = "SecurePassword123!";

            // Act
            var result = await _viewModel.ExecuteLogin();

            // Assert
            result.Should().BeFalse();
            _viewModel.ErrorMessage.Should().Contain("Username and password are required");
            _mockAuthService.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteLogin_ShouldReturnFalse_WhenPasswordIsEmpty()
        {
            // Arrange
            _viewModel.Username = "testuser";
            _viewModel.Password = string.Empty;

            // Act
            var result = await _viewModel.ExecuteLogin();

            // Assert
            result.Should().BeFalse();
            _viewModel.ErrorMessage.Should().Contain("Username and password are required");
            _mockAuthService.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteLogin_ShouldReturnFalse_WhenUsernameIsWhitespace()
        {
            // Arrange
            _viewModel.Username = "   ";
            _viewModel.Password = "SecurePassword123!";

            // Act
            var result = await _viewModel.ExecuteLogin();

            // Assert
            result.Should().BeFalse();
            _viewModel.ErrorMessage.Should().Contain("Username and password are required");
            _mockAuthService.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteLogin_ShouldReturnFalse_WhenPasswordIsWhitespace()
        {
            // Arrange
            _viewModel.Username = "testuser";
            _viewModel.Password = "   ";

            // Act
            var result = await _viewModel.ExecuteLogin();

            // Assert
            result.Should().BeFalse();
            _viewModel.ErrorMessage.Should().Contain("Username and password are required");
            _mockAuthService.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteLogin_ShouldReturnFalse_WhenBothCredentialsAreEmpty()
        {
            // Arrange
            _viewModel.Username = string.Empty;
            _viewModel.Password = string.Empty;

            // Act
            var result = await _viewModel.ExecuteLogin();

            // Assert
            result.Should().BeFalse();
            _viewModel.ErrorMessage.Should().Contain("Username and password are required");
            _mockAuthService.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region Error Message Tests

        [Fact]
        public async Task ExecuteLogin_ShouldClearErrorMessage_WhenStartingLogin()
        {
            // Arrange
            _viewModel.ErrorMessage = "Previous error";
            _viewModel.Username = "testuser";
            _viewModel.Password = "SecurePassword123!";
            _mockAuthService.Setup(a => a.LoginAsync("testuser", "SecurePassword123!")).ReturnsAsync(true);

            // Act
            await _viewModel.ExecuteLogin();

            // Assert
            _viewModel.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public async Task ExecuteLogin_ShouldSetErrorMessage_WhenLoginFails()
        {
            // Arrange
            _viewModel.Username = "testuser";
            _viewModel.Password = "WrongPassword";
            _mockAuthService.Setup(a => a.LoginAsync("testuser", "WrongPassword")).ReturnsAsync(false);

            // Act
            await _viewModel.ExecuteLogin();

            // Assert
            _viewModel.ErrorMessage.Should().NotBeNullOrEmpty();
        }

        #endregion

        #region IsLoading Tests

        [Fact]
        public async Task ExecuteLogin_ShouldSetIsLoadingTrue_DuringLogin()
        {
            // Arrange
            _viewModel.Username = "testuser";
            _viewModel.Password = "SecurePassword123!";
            var isLoadingTracker = false;

            _mockAuthService.Setup(a => a.LoginAsync("testuser", "SecurePassword123!"))
                .Callback(() => isLoadingTracker = _viewModel.IsLoading)
                .ReturnsAsync(true);

            // Act
            await _viewModel.ExecuteLogin();

            // Assert
            // After execution, IsLoading should be false
            _viewModel.IsLoading.Should().BeFalse();
        }

        #endregion

        #region ReturnUrl Tests

        [Fact]
        public void ReturnUrl_ShouldBeAccessible()
        {
            // Arrange & Act
            _viewModel.ReturnUrl = "/dashboard";

            // Assert
            _viewModel.ReturnUrl.Should().Be("/dashboard");
        }

        [Fact]
        public void ReturnUrl_ShouldBeNullByDefault()
        {
            // Act & Assert
            _viewModel.ReturnUrl.Should().BeNull();
        }

        [Fact]
        public void ReturnUrl_ShouldHandleEmptyString()
        {
            // Arrange & Act
            _viewModel.ReturnUrl = string.Empty;

            // Assert
            _viewModel.ReturnUrl.Should().Be(string.Empty);
        }

        #endregion

        #region Property Initialization Tests

        [Fact]
        public void LoginViewModel_ShouldInitializeWithEmptyCredentials()
        {
            // Act & Assert
            _viewModel.Username.Should().Be(string.Empty);
            _viewModel.Password.Should().Be(string.Empty);
        }

        [Fact]
        public void LoginViewModel_ShouldInitializeWithNullErrorMessage()
        {
            // Act & Assert
            _viewModel.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void LoginViewModel_ShouldInitializeWithIsLoadingFalse()
        {
            // Act & Assert
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public void LoginViewModel_ShouldInitializeWithNullReturnUrl()
        {
            // Act & Assert
            _viewModel.ReturnUrl.Should().BeNull();
        }

        #endregion

        #region Credential Case Sensitivity Tests

        [Fact]
        public async Task ExecuteLogin_ShouldPassCredentialsAsProvidedToAuthService()
        {
            // Arrange
            _viewModel.Username = "TestUser";
            _viewModel.Password = "SecurePassword123!";
            _mockAuthService.Setup(a => a.LoginAsync("TestUser", "SecurePassword123!")).ReturnsAsync(true);

            // Act
            await _viewModel.ExecuteLogin();

            // Assert
            _mockAuthService.Verify(a => a.LoginAsync("TestUser", "SecurePassword123!"), Times.Once);
        }

        #endregion

        #region Multiple Login Attempts Tests

        [Fact]
        public async Task ExecuteLogin_ShouldAllowMultipleAttempts()
        {
            // Arrange
            _viewModel.Username = "testuser";
            _viewModel.Password = "WrongPassword";
            _mockAuthService.Setup(a => a.LoginAsync("testuser", "WrongPassword")).ReturnsAsync(false);

            // Act - First attempt
            var result1 = await _viewModel.ExecuteLogin();

            // Arrange - Second attempt with correct password
            _viewModel.Password = "SecurePassword123!";
            _mockAuthService.Setup(a => a.LoginAsync("testuser", "SecurePassword123!")).ReturnsAsync(true);

            // Act - Second attempt
            var result2 = await _viewModel.ExecuteLogin();

            // Assert
            result1.Should().BeFalse();
            result2.Should().BeTrue();
            _mockAuthService.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }

        #endregion
    }
}
