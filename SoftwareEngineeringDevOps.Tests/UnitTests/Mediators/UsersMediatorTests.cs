using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SoftwareEngineeringDevOps.App.Users;
using SoftwareEngineeringDevOps.App.Users.Repository;
using SoftwareEngineeringDevOps.Tests.TestUtilities;

namespace SoftwareEngineeringDevOps.Tests.UnitTests.Mediators
{
    /// <summary>
    /// Unit tests for UsersMediator covering CRUD operations with mocked dependencies
    /// </summary>
    public class UsersMediatorTests
    {
        private readonly Mock<IUsersRepository> _mockRepository;
        private readonly Mock<ILogger<UsersMediator>> _mockLogger;
        private readonly UsersMediator _mediator;

        public UsersMediatorTests()
        {
            _mockRepository = new Mock<IUsersRepository>();
            _mockLogger = new Mock<ILogger<UsersMediator>>();
            _mediator = new UsersMediator(_mockRepository.Object, _mockLogger.Object);
        }

        #region GetAllUsers Tests

        [Fact]
        public async Task GetAllUsers_ShouldReturnAllUsers_WhenUsersExist()
        {
            // Arrange
            var expectedUsers = new List<IUser>
            {
                MockDataFactory.Users.CreateValid(1, "user1"),
                MockDataFactory.Users.CreateValid(2, "user2"),
                MockDataFactory.Users.CreateValid(3, "user3", isAdmin: true)
            };
            _mockRepository.Setup(r => r.ListAll()).Returns(expectedUsers);

            // Act
            var result = await _mediator.GetAllUsers();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedUsers);
            _mockRepository.Verify(r => r.ListAll(), Times.Once);
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnEmptyCollection_WhenNoUsersExist()
        {
            // Arrange
            _mockRepository.Setup(r => r.ListAll()).Returns(Enumerable.Empty<IUser>());

            // Act
            var result = await _mediator.GetAllUsers();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
            _mockRepository.Verify(r => r.ListAll(), Times.Once);
        }

        #endregion

        #region GetUserById Tests

        [Fact]
        public async Task GetUserById_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            const long userId = 1;
            var expectedUser = MockDataFactory.Users.CreateValid(userId);
            _mockRepository.Setup(r => r.GetById(userId)).Returns(expectedUser);

            // Act
            var result = await _mediator.GetUserById(userId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(expectedUser);
            _mockRepository.Verify(r => r.GetById(userId), Times.Once);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            const long nonExistentId = 999;
            _mockRepository.Setup(r => r.GetById(nonExistentId)).Returns((IUser?)null);

            // Act
            var result = await _mediator.GetUserById(nonExistentId);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetById(nonExistentId), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(long.MinValue)]
        public async Task GetUserById_ShouldHandleInvalidIds(long invalidId)
        {
            // Arrange
            _mockRepository.Setup(r => r.GetById(invalidId)).Returns((IUser?)null);

            // Act
            var result = await _mediator.GetUserById(invalidId);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region GetUserByUsername Tests

        [Fact]
        public async Task GetUserByUsername_ShouldReturnUser_WhenUserExists()
        {
            // Arrange
            const string username = "testuser";
            var expectedUser = MockDataFactory.Users.CreateValid(1, username);
            _mockRepository.Setup(r => r.GetByUsername(username)).Returns(expectedUser);

            // Act
            var result = await _mediator.GetUserByUsername(username);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be(username);
            result.Should().BeEquivalentTo(expectedUser);
            _mockRepository.Verify(r => r.GetByUsername(username), Times.Once);
        }

        [Fact]
        public async Task GetUserByUsername_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            const string nonExistentUsername = "nonexistent";
            _mockRepository.Setup(r => r.GetByUsername(nonExistentUsername)).Returns((IUser?)null);

            // Act
            var result = await _mediator.GetUserByUsername(nonExistentUsername);

            // Assert
            result.Should().BeNull();
            _mockRepository.Verify(r => r.GetByUsername(nonExistentUsername), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task GetUserByUsername_ShouldHandleNullOrEmptyUsername(string username)
        {
            // Arrange
            _mockRepository.Setup(r => r.GetByUsername(username)).Returns((IUser?)null);

            // Act
            var result = await _mediator.GetUserByUsername(username);

            // Assert
            result.Should().BeNull();
        }

        #endregion

        #region Insert Tests

        [Fact]
        public async Task Insert_ShouldReturnCreatedUser_WhenValidDataProvided()
        {
            // Arrange
            var newUser = MockDataFactory.Users.CreateValidNew("newuser");
            var expectedUser = MockDataFactory.Users.CreateValid(1, "newuser");
            _mockRepository.Setup(r => r.Insert(newUser)).Returns(expectedUser);

            // Act
            var result = await _mediator.Insert(newUser);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be(newUser.Username);
            result.FirstName.Should().Be(newUser.FirstName);
            result.LastName.Should().Be(newUser.LastName);
            _mockRepository.Verify(r => r.Insert(newUser), Times.Once);
        }

        [Fact]
        public async Task Insert_ShouldPreserveAdminRole_WhenAdminUserCreated()
        {
            // Arrange
            var newUser = MockDataFactory.Users.CreateValidNew("admin", isAdmin: true);
            var expectedUser = MockDataFactory.Users.CreateValid(1, "admin", isAdmin: true);
            _mockRepository.Setup(r => r.Insert(newUser)).Returns(expectedUser);

            // Act
            var result = await _mediator.Insert(newUser);

            // Assert
            result.IsAdmin.Should().BeTrue();
            result.IsEditor.Should().BeFalse();
            _mockRepository.Verify(r => r.Insert(newUser), Times.Once);
        }

        [Fact]
        public async Task Insert_ShouldPreserveEditorRole_WhenEditorUserCreated()
        {
            // Arrange
            var newUser = MockDataFactory.Users.CreateValidNew("editor", isEditor: true);
            var expectedUser = MockDataFactory.Users.CreateValid(1, "editor", isEditor: true);
            _mockRepository.Setup(r => r.Insert(newUser)).Returns(expectedUser);

            // Act
            var result = await _mediator.Insert(newUser);

            // Assert
            result.IsEditor.Should().BeTrue();
            result.IsAdmin.Should().BeFalse();
            _mockRepository.Verify(r => r.Insert(newUser), Times.Once);
        }

        #endregion

        #region Update Tests

        [Fact]
        public async Task Update_ShouldReturnUpdatedUser_WhenValidDataProvided()
        {
            // Arrange
            var existingUser = MockDataFactory.Users.CreateValid(1, "testuser");
            var editUser = new EditUser(existingUser)
            {
                FirstName = "Updated",
                LastName = "Name"
            };
            var updatedUser = MockDataFactory.Users.CreateValid(1, "testuser");
            _mockRepository.Setup(r => r.Update(editUser)).Returns(updatedUser);

            // Act
            var result = await _mediator.Update(editUser);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be(existingUser.Username);
            _mockRepository.Verify(r => r.Update(editUser), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldPreserveId_WhenUpdatingUser()
        {
            // Arrange
            const long userId = 5;
            var existingUser = MockDataFactory.Users.CreateValid(userId, "testuser");
            var editUser = new EditUser(existingUser);
            var updatedUser = MockDataFactory.Users.CreateValid(userId, "testuser");
            _mockRepository.Setup(r => r.Update(editUser)).Returns(updatedUser);

            // Act
            var result = await _mediator.Update(editUser);

            // Assert
            result.Id.Should().Be(userId);
            _mockRepository.Verify(r => r.Update(editUser), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldPreserveUsername_WhenUpdatingUser()
        {
            // Arrange
            const string username = "unchangedusername";
            var existingUser = MockDataFactory.Users.CreateValid(1, username);
            var editUser = new EditUser(existingUser);
            var updatedUser = MockDataFactory.Users.CreateValid(1, username);
            _mockRepository.Setup(r => r.Update(editUser)).Returns(updatedUser);

            // Act
            var result = await _mediator.Update(editUser);

            // Assert
            result.Username.Should().Be(username);
            _mockRepository.Verify(r => r.Update(editUser), Times.Once);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public async Task Delete_ShouldCallRepositoryDelete_WhenValidIdProvided()
        {
            // Arrange
            const long userId = 1;
            _mockRepository.Setup(r => r.Delete(userId));

            // Act
            await _mediator.Delete(userId);

            // Assert
            _mockRepository.Verify(r => r.Delete(userId), Times.Once);
        }

        [Fact]
        public async Task Delete_ShouldNotThrow_WhenDeletingNonExistentUser()
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
        [InlineData(long.MinValue)]
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
