using FluentAssertions;
using Moq;
using SoftwareEngineeringDevOps.App.Auth;
using SoftwareEngineeringDevOps.App.Users;
using SoftwareEngineeringDevOps.Components.ViewModels;
using SoftwareEngineeringDevOps.Tests.TestUtilities;

namespace SoftwareEngineeringDevOps.Tests.UnitTests.ViewModels
{
    /// <summary>
    /// Comprehensive tests for RegisterViewModel covering registration, username generation, validation, and error handling
    /// </summary>
    public class RegisterViewModelTests
    {
        private readonly Mock<IUsersMediator> _mockUsersMediator;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly RegisterViewModel _viewModel;

        public RegisterViewModelTests()
        {
            _mockUsersMediator = new Mock<IUsersMediator>();
            _mockAuthService = new Mock<IAuthService>();
            _viewModel = new RegisterViewModel(_mockUsersMediator.Object, _mockAuthService.Object);
        }

        #region ExecuteRegister Tests - Happy Path

        [Fact]
        public async Task ExecuteRegister_ShouldSucceed_WhenValidDataProvided()
        {
            // Arrange
            _viewModel.Username = "newuser";
            _viewModel.Password = "SecurePassword123!";
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";

            _mockUsersMediator.Setup(m => m.GetUserByUsername("newuser")).ReturnsAsync((IUser?)null);
            var newUser = new NewUser
            {
                Username = "newuser",
                Password = "SecurePassword123!",
                FirstName = "John",
                LastName = "Doe",
                IsAdmin = false,
                IsEditor = false
            };
            _mockUsersMediator.Setup(m => m.Insert(It.IsAny<NewUser>())).ReturnsAsync(MockDataFactory.Users.CreateValid(1, "newuser"));
            _mockAuthService.Setup(a => a.LoginAsync("newuser", "SecurePassword123!")).ReturnsAsync(true);

            // Act
            var result = await _viewModel.ExecuteRegister();

            // Assert
            result.Should().BeTrue();
            _viewModel.ErrorMessage.Should().BeNull();
            _viewModel.ValidationErrors.Should().BeEmpty();
            _viewModel.IsLoading.Should().BeFalse();
            _mockUsersMediator.Verify(m => m.Insert(It.IsAny<NewUser>()), Times.Once);
            _mockAuthService.Verify(a => a.LoginAsync("newuser", "SecurePassword123!"), Times.Once);
        }

        [Fact]
        public async Task ExecuteRegister_ShouldSetIsLoadingFalse_WhenRegistrationSucceeds()
        {
            // Arrange
            _viewModel.Username = "newuser";
            _viewModel.Password = "SecurePassword123!";
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";

            _mockUsersMediator.Setup(m => m.GetUserByUsername("newuser")).ReturnsAsync((IUser?)null);
            _mockUsersMediator.Setup(m => m.Insert(It.IsAny<NewUser>())).ReturnsAsync(MockDataFactory.Users.CreateValid(1, "newuser"));
            _mockAuthService.Setup(a => a.LoginAsync("newuser", "SecurePassword123!")).ReturnsAsync(true);

            // Act
            await _viewModel.ExecuteRegister();

            // Assert
            _viewModel.IsLoading.Should().BeFalse();
        }

        #endregion

        #region ExecuteRegister Tests - Validation

        [Fact]
        public async Task ExecuteRegister_ShouldReturnFalse_WhenUsernameAlreadyExists()
        {
            // Arrange
            var existingUser = MockDataFactory.Users.CreateValid(1, "existinguser");
            _viewModel.Username = "existinguser";
            _viewModel.Password = "SecurePassword123!";
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";

            _mockUsersMediator.Setup(m => m.GetUserByUsername("existinguser")).ReturnsAsync(existingUser);

            // Act
            var result = await _viewModel.ExecuteRegister();

            // Assert
            result.Should().BeFalse();
            _viewModel.ErrorMessage.Should().Contain("Username already exists");
            _viewModel.IsLoading.Should().BeFalse();
            _mockUsersMediator.Verify(m => m.Insert(It.IsAny<NewUser>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteRegister_ShouldReturnFalse_WhenValidationFails()
        {
            // Arrange
            _viewModel.Username = "user"; // Potentially invalid per validator
            _viewModel.Password = "weak"; // Potentially invalid - weak password
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";

            // Act
            var result = await _viewModel.ExecuteRegister();

            // Assert
            result.Should().BeFalse();
            _viewModel.ValidationErrors.Should().NotBeEmpty();
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task ExecuteRegister_ShouldSetIsLoadingFalse_WhenValidationFails()
        {
            // Arrange
            _viewModel.Username = "u"; // Invalid username
            _viewModel.Password = "weak";
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";

            // Act
            await _viewModel.ExecuteRegister();

            // Assert
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task ExecuteRegister_ShouldClearPreviousErrors_WhenStartingRegistration()
        {
            // Arrange
            _viewModel.ErrorMessage = "Previous error";
            _viewModel.ValidationErrors.Add("Previous validation error");
            _viewModel.Username = "newuser";
            _viewModel.Password = "SecurePassword123!";
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";

            _mockUsersMediator.Setup(m => m.GetUserByUsername("newuser")).ReturnsAsync((IUser?)null);
            _mockUsersMediator.Setup(m => m.Insert(It.IsAny<NewUser>())).ReturnsAsync(MockDataFactory.Users.CreateValid(1, "newuser"));
            _mockAuthService.Setup(a => a.LoginAsync("newuser", "SecurePassword123!")).ReturnsAsync(true);

            // Act
            await _viewModel.ExecuteRegister();

            // Assert
            _viewModel.ErrorMessage.Should().BeNull();
            _viewModel.ValidationErrors.Should().BeEmpty();
        }

        #endregion

        #region GenerateUsername Tests - Basic Generation

        [Fact]
        public async Task GenerateUsernameAsync_ShouldGenerateUsernameFromFirstAndLastName()
        {
            // Arrange
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";
            _mockUsersMediator.Setup(m => m.GetUserByUsername("johndoe")).ReturnsAsync((IUser?)null);

            // Act
            await _viewModel.GenerateUsernameAsync();

            // Assert
            _viewModel.Username.Should().Be("johndoe");
        }

        [Fact]
        public async Task GenerateUsernameAsync_ShouldConvertToLowercase()
        {
            // Arrange
            _viewModel.FirstName = "JOHN";
            _viewModel.LastName = "DOE";
            _mockUsersMediator.Setup(m => m.GetUserByUsername("johndoe")).ReturnsAsync((IUser?)null);

            // Act
            await _viewModel.GenerateUsernameAsync();

            // Assert
            _viewModel.Username.Should().Be("johndoe");
        }

        [Fact]
        public async Task GenerateUsernameAsync_ShouldRemoveSpaces()
        {
            // Arrange
            _viewModel.FirstName = "John Michael";
            _viewModel.LastName = "Van Doe";
            _mockUsersMediator.Setup(m => m.GetUserByUsername("johnmichaelvandoe")).ReturnsAsync((IUser?)null);

            // Act
            await _viewModel.GenerateUsernameAsync();

            // Assert
            _viewModel.Username.Should().Be("johnmichaelvandoe");
        }

        [Fact]
        public async Task GenerateUsernameAsync_ShouldTrimWhitespace()
        {
            // Arrange
            _viewModel.FirstName = "  John  ";
            _viewModel.LastName = "  Doe  ";
            _mockUsersMediator.Setup(m => m.GetUserByUsername("johndoe")).ReturnsAsync((IUser?)null);

            // Act
            await _viewModel.GenerateUsernameAsync();

            // Assert
            _viewModel.Username.Should().Be("johndoe");
        }

        #endregion

        #region GenerateUsername Tests - Empty Input

        [Fact]
        public async Task GenerateUsernameAsync_ShouldClearUsername_WhenBothNamesAreEmpty()
        {
            // Arrange
            _viewModel.FirstName = string.Empty;
            _viewModel.LastName = string.Empty;
            _viewModel.Username = "previoususername";

            // Act
            await _viewModel.GenerateUsernameAsync();

            // Assert
            _viewModel.Username.Should().Be(string.Empty);
        }

        [Fact]
        public async Task GenerateUsernameAsync_ShouldClearUsername_WhenBothNamesAreWhitespace()
        {
            // Arrange
            _viewModel.FirstName = "   ";
            _viewModel.LastName = "   ";
            _viewModel.Username = "previoususername";

            // Act
            await _viewModel.GenerateUsernameAsync();

            // Assert
            _viewModel.Username.Should().Be(string.Empty);
        }

        [Fact]
        public async Task GenerateUsernameAsync_ShouldGenerateFromFirstNameOnly_WhenLastNameIsEmpty()
        {
            // Arrange
            _viewModel.FirstName = "John";
            _viewModel.LastName = string.Empty;
            _mockUsersMediator.Setup(m => m.GetUserByUsername("john")).ReturnsAsync((IUser?)null);

            // Act
            await _viewModel.GenerateUsernameAsync();

            // Assert
            _viewModel.Username.Should().Be("john");
        }

        [Fact]
        public async Task GenerateUsernameAsync_ShouldGenerateFromLastNameOnly_WhenFirstNameIsEmpty()
        {
            // Arrange
            _viewModel.FirstName = string.Empty;
            _viewModel.LastName = "Doe";
            _mockUsersMediator.Setup(m => m.GetUserByUsername("doe")).ReturnsAsync((IUser?)null);

            // Act
            await _viewModel.GenerateUsernameAsync();

            // Assert
            _viewModel.Username.Should().Be("doe");
        }

        #endregion

        #region GenerateUsername Tests - Manual Edit Check

        [Fact]
        public async Task GenerateUsernameAsync_ShouldNotOverwrite_WhenUsernameIsManuallyEdited()
        {
            // Arrange
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";
            _viewModel.Username = "customusername";
            _viewModel.IsUsernameManuallyEdited = true;

            // Act
            await _viewModel.GenerateUsernameAsync();

            // Assert
            _viewModel.Username.Should().Be("customusername");
            _mockUsersMediator.Verify(m => m.GetUserByUsername(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GenerateUsernameAsync_ShouldOverwrite_WhenUsernameIsNotManuallyEdited()
        {
            // Arrange
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";
            _viewModel.Username = "oldusername";
            _viewModel.IsUsernameManuallyEdited = false;
            _mockUsersMediator.Setup(m => m.GetUserByUsername("johndoe")).ReturnsAsync((IUser?)null);

            // Act
            await _viewModel.GenerateUsernameAsync();

            // Assert
            _viewModel.Username.Should().Be("johndoe");
        }

        #endregion

        #region GenerateUsername Tests - Uniqueness Handling

        [Fact]
        public async Task GenerateUsernameAsync_ShouldAddSuffix_WhenUsernameAlreadyExists()
        {
            // Arrange
            var existingUser = MockDataFactory.Users.CreateValid(1, "johndoe");
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";

            var callCount = 0;
            _mockUsersMediator.Setup(m => m.GetUserByUsername(It.IsAny<string>()))
                .Returns((string username) =>
                {
                    callCount++;
                    if (username == "johndoe" && callCount == 1)
                    {
                        return Task.FromResult<IUser?>(existingUser);
                    }
                    return Task.FromResult<IUser?>(null);
                });

            // Act
            await _viewModel.GenerateUsernameAsync();

            // Assert
            _viewModel.Username.Should().Be("johndoe02");
        }

        [Fact]
        public async Task GenerateUsernameAsync_ShouldIncrementSuffix_WhenMultipleUsersExist()
        {
            // Arrange
            var user1 = MockDataFactory.Users.CreateValid(1, "johndoe");
            var user2 = MockDataFactory.Users.CreateValid(2, "johndoe02");
            var user3 = MockDataFactory.Users.CreateValid(3, "johndoe03");
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";

            var callCount = 0;
            _mockUsersMediator.Setup(m => m.GetUserByUsername(It.IsAny<string>()))
                .Returns((string username) =>
                {
                    callCount++;
                    if (username == "johndoe" && callCount == 1) return Task.FromResult<IUser?>(user1);
                    if (username == "johndoe02" && callCount == 2) return Task.FromResult<IUser?>(user2);
                    if (username == "johndoe03" && callCount == 3) return Task.FromResult<IUser?>(user3);
                    return Task.FromResult<IUser?>(null);
                });

            // Act
            await _viewModel.GenerateUsernameAsync();

            // Assert
            _viewModel.Username.Should().Be("johndoe04");
        }

        #endregion

        #region Property Initialization Tests

        [Fact]
        public void RegisterViewModel_ShouldInitializeWithEmptyFields()
        {
            // Act & Assert
            _viewModel.Username.Should().Be(string.Empty);
            _viewModel.Password.Should().Be(string.Empty);
            _viewModel.FirstName.Should().Be(string.Empty);
            _viewModel.LastName.Should().Be(string.Empty);
        }

        [Fact]
        public void RegisterViewModel_ShouldInitializeWithNullErrorMessage()
        {
            // Act & Assert
            _viewModel.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void RegisterViewModel_ShouldInitializeWithEmptyValidationErrors()
        {
            // Act & Assert
            _viewModel.ValidationErrors.Should().BeEmpty();
        }

        [Fact]
        public void RegisterViewModel_ShouldInitializeWithIsLoadingFalse()
        {
            // Act & Assert
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public void RegisterViewModel_ShouldInitializeWithIsUsernameManuallyEditedFalse()
        {
            // Act & Assert
            _viewModel.IsUsernameManuallyEdited.Should().BeFalse();
        }

        #endregion

        #region ExecuteRegister Tests - Integration

        [Fact]
        public async Task ExecuteRegister_ShouldNotLoginOnValidationFailure()
        {
            // Arrange
            _viewModel.Username = "u"; // Invalid
            _viewModel.Password = "weak";
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";

            // Act
            await _viewModel.ExecuteRegister();

            // Assert
            _mockAuthService.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteRegister_ShouldNotLoginWhenUsernameExists()
        {
            // Arrange
            var existingUser = MockDataFactory.Users.CreateValid(1, "existing");
            _viewModel.Username = "existing";
            _viewModel.Password = "SecurePassword123!";
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";

            _mockUsersMediator.Setup(m => m.GetUserByUsername("existing")).ReturnsAsync(existingUser);

            // Act
            await _viewModel.ExecuteRegister();

            // Assert
            _mockAuthService.Verify(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockUsersMediator.Verify(m => m.Insert(It.IsAny<NewUser>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteRegister_ShouldCreateNonAdminUser()
        {
            // Arrange
            _viewModel.Username = "newuser";
            _viewModel.Password = "SecurePassword123!";
            _viewModel.FirstName = "John";
            _viewModel.LastName = "Doe";

            _mockUsersMediator.Setup(m => m.GetUserByUsername("newuser")).ReturnsAsync((IUser?)null);

            NewUser? capturedUser = null;
            _mockUsersMediator.Setup(m => m.Insert(It.IsAny<NewUser>()))
                .Callback((NewUser u) => capturedUser = u)
                .ReturnsAsync(MockDataFactory.Users.CreateValid(1, "newuser"));

            _mockAuthService.Setup(a => a.LoginAsync("newuser", "SecurePassword123!")).ReturnsAsync(true);

            // Act
            await _viewModel.ExecuteRegister();

            // Assert
            capturedUser.Should().NotBeNull();
            capturedUser!.IsAdmin.Should().BeFalse();
            capturedUser!.IsEditor.Should().BeFalse();
        }

        #endregion
    }
}
