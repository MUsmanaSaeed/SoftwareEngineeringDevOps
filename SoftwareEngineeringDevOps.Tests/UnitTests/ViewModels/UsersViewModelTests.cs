using FluentAssertions;
using Moq;
using SoftwareEngineeringDevOps.App.Auth;
using SoftwareEngineeringDevOps.App.Users;
using SoftwareEngineeringDevOps.Components.ViewModels;
using SoftwareEngineeringDevOps.Tests.TestUtilities;

namespace SoftwareEngineeringDevOps.Tests.UnitTests.ViewModels
{
    /// <summary>
    /// Comprehensive tests for UsersViewModel covering CRUD operations, validation, filtering, and security
    /// </summary>
    public class UsersViewModelTests
    {
        private readonly Mock<IUsersMediator> _mockUsersMediator;
        private readonly Mock<IAuthService> _mockAuthService;
        private readonly UsersViewModel _viewModel;

        public UsersViewModelTests()
        {
            _mockUsersMediator = new Mock<IUsersMediator>();
            _mockAuthService = new Mock<IAuthService>();

            // Setup default current user
            _mockAuthService.Setup(a => a.CurrentUser).Returns(MockDataFactory.Users.CreateAdmin());

            _viewModel = new UsersViewModel(_mockUsersMediator.Object, _mockAuthService.Object);
        }

        #region LoadUsers Tests

        [Fact]
        public async Task LoadUsers_ShouldLoadAllUsers_WhenCalled()
        {
            // Arrange
            var users = new List<IUser>
            {
                MockDataFactory.Users.CreateValid(1, "user1"),
                MockDataFactory.Users.CreateValid(2, "user2"),
                MockDataFactory.Users.CreateValid(3, "user3")
            };
            _mockUsersMediator.Setup(m => m.GetAllUsers()).ReturnsAsync(users);

            // Act
            await _viewModel.LoadUsers();

            // Assert
            _viewModel.Users.Should().HaveCount(3);
            _viewModel.IsLoading.Should().BeFalse();
            _mockUsersMediator.Verify(m => m.GetAllUsers(), Times.Once);
        }

        [Fact]
        public async Task LoadUsers_ShouldSetIsLoadingCorrectly()
        {
            // Arrange
            _mockUsersMediator.Setup(m => m.GetAllUsers()).ReturnsAsync(Enumerable.Empty<IUser>());

            // Act
            await _viewModel.LoadUsers();

            // Assert
            _viewModel.IsLoading.Should().BeFalse();
        }

        [Fact]
        public async Task LoadUsers_ShouldHandleEmptyCollections()
        {
            // Arrange
            _mockUsersMediator.Setup(m => m.GetAllUsers()).ReturnsAsync(Enumerable.Empty<IUser>());

            // Act
            await _viewModel.LoadUsers();

            // Assert
            _viewModel.Users.Should().BeEmpty();
        }

        #endregion

        #region SelectUser Tests

        [Fact]
        public void SelectUser_ShouldSetSelectedUser()
        {
            // Arrange
            var user = MockDataFactory.Users.CreateValid(1, "testuser");

            // Act
            _viewModel.SelectUser(user);

            // Assert
            _viewModel.SelectedUser.Should().Be(user);
        }

        [Fact]
        public void SelectUser_ShouldReplaceExistingSelection()
        {
            // Arrange
            var user1 = MockDataFactory.Users.CreateValid(1, "user1");
            var user2 = MockDataFactory.Users.CreateValid(2, "user2");
            _viewModel.SelectUser(user1);

            // Act
            _viewModel.SelectUser(user2);

            // Assert
            _viewModel.SelectedUser.Should().Be(user2);
        }

        #endregion

        #region Modal Tests

        [Fact]
        public void OpenAddModal_ShouldShowModal()
        {
            // Act
            _viewModel.OpenAddModal();

            // Assert
            _viewModel.ShowAddModal.Should().BeTrue();
            _viewModel.ValidationErrors.Should().BeEmpty();
            _viewModel.NewUserModel.Should().NotBeNull();
        }

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
        }

        [Fact]
        public void OpenEditModal_ShouldShowModalWithUserData()
        {
            // Arrange
            var user = MockDataFactory.Users.CreateValid(1, "testuser");

            // Act
            _viewModel.OpenEditModal(user);

            // Assert
            _viewModel.ShowEditModal.Should().BeTrue();
            _viewModel.EditUserModel.Should().NotBeNull();
            _viewModel.EditUserModel?.Id.Should().Be(user.Id);
        }

        [Fact]
        public void CloseEditModal_ShouldHideModal()
        {
            // Arrange
            var user = MockDataFactory.Users.CreateValid(1, "testuser");
            _viewModel.OpenEditModal(user);

            // Act
            _viewModel.CloseEditModal();

            // Assert
            _viewModel.ShowEditModal.Should().BeFalse();
            _viewModel.ValidationErrors.Should().BeEmpty();
        }

        #endregion

        #region FilteredUsers Tests

        [Fact]
        public void FilteredUsers_ShouldReturnAllUsers_WhenSearchTermIsEmpty()
        {
            // Arrange
            var users = new List<IUser>
            {
                MockDataFactory.Users.CreateValid(1, "alice"),
                MockDataFactory.Users.CreateValid(2, "bob"),
                MockDataFactory.Users.CreateValid(3, "charlie")
            };
            _viewModel.Users = users;
            _viewModel.UsersSearchTerm = string.Empty;

            // Act
            var result = _viewModel.FilteredUsers;

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public void FilteredUsers_ShouldFilterByUsername()
        {
            // Arrange
            var users = new List<IUser>
            {
                MockDataFactory.Users.CreateValid(1, "alice"),
                MockDataFactory.Users.CreateValid(2, "bob"),
                MockDataFactory.Users.CreateValid(3, "charlie")
            };
            _viewModel.Users = users;
            _viewModel.UsersSearchTerm = "bob";

            // Act
            var result = _viewModel.FilteredUsers;

            // Assert
            result.Should().HaveCount(1);
            result.First().Username.Should().Be("bob");
        }

        [Fact]
        public void FilteredUsers_ShouldFilterByFirstName()
        {
            // Arrange
            var users = new List<IUser>
            {
                MockDataFactory.Users.CreateValid(1, "alice"),
                MockDataFactory.Users.CreateValid(2, "bob"),
                MockDataFactory.Users.CreateValid(3, "charlie")
            };
            _viewModel.Users = users;
            _viewModel.UsersSearchTerm = "Test"; // All use "Test" as first name

            // Act
            var result = _viewModel.FilteredUsers;

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact]
        public void FilteredUsers_ShouldFilterByRole()
        {
            // Arrange
            var users = new List<IUser>
            {
                MockDataFactory.Users.CreateValid(1, "admin", isAdmin: true),
                MockDataFactory.Users.CreateValid(2, "editor", isEditor: true),
                MockDataFactory.Users.CreateValid(3, "standard")
            };
            _viewModel.Users = users;
            _viewModel.UsersSearchTerm = "Admin";

            // Act
            var result = _viewModel.FilteredUsers;

            // Assert
            result.Should().HaveCount(1);
            result.First().IsAdmin.Should().BeTrue();
        }

        [Fact]
        public void FilteredUsers_ShouldBeCaseInsensitive()
        {
            // Arrange
            var users = new List<IUser>
            {
                MockDataFactory.Users.CreateValid(1, "TestUser")
            };
            _viewModel.Users = users;
            _viewModel.UsersSearchTerm = "testuser";

            // Act
            var result = _viewModel.FilteredUsers;

            // Assert
            result.Should().HaveCount(1);
        }

        [Fact]
        public void FilteredUsers_ShouldReturnOrderedByUsername()
        {
            // Arrange
            var users = new List<IUser>
            {
                MockDataFactory.Users.CreateValid(1, "charlie"),
                MockDataFactory.Users.CreateValid(2, "alice"),
                MockDataFactory.Users.CreateValid(3, "bob")
            };
            _viewModel.Users = users;
            _viewModel.UsersSearchTerm = string.Empty;

            // Act
            var result = _viewModel.FilteredUsers.ToList();

            // Assert
            result[0].Username.Should().Be("alice");
            result[1].Username.Should().Be("bob");
            result[2].Username.Should().Be("charlie");
        }

        #endregion

        #region AddUser Tests - Happy Path

        [Fact]
        public async Task AddUser_ShouldAddUser_WhenValidDataProvided()
        {
            // Arrange
            var newUser = MockDataFactory.Users.CreateValidNew("newuser");
            _viewModel.NewUserModel = newUser;

            var createdUser = MockDataFactory.Users.CreateValid(1, "newuser");
            var allUsers = new List<IUser> { createdUser };

            _mockUsersMediator.Setup(m => m.Insert(newUser)).ReturnsAsync(createdUser);
            _mockUsersMediator.Setup(m => m.GetAllUsers()).ReturnsAsync(allUsers);

            // Act
            var result = await _viewModel.AddUser();

            // Assert
            result.Should().BeTrue();
            _viewModel.ShowAddModal.Should().BeFalse();
            _viewModel.ValidationErrors.Should().BeEmpty();
            _mockUsersMediator.Verify(m => m.Insert(newUser), Times.Once);
        }

        #endregion

        #region AddUser Tests - Validation

        [Fact]
        public async Task AddUser_ShouldReturnFalse_WhenUsernameAlreadyExists()
        {
            // Arrange
            var existingUser = MockDataFactory.Users.CreateValid(1, "existinguser");
            var newUser = new NewUser { Username = "existinguser", Password = "SecurePassword123!", FirstName = "Test", LastName = "User" };
            _viewModel.NewUserModel = newUser;
            _viewModel.Users = new List<IUser> { existingUser };

            // Act
            var result = await _viewModel.AddUser();

            // Assert
            result.Should().BeFalse();
            _viewModel.ValidationErrors.Should().Contain("A user with that username already exists.");
            _viewModel.ShowAddModal.Should().BeTrue();
        }

        #endregion

        #region UpdateUser Tests - Happy Path

        [Fact]
        public async Task UpdateUser_ShouldUpdateUser_WhenValidDataProvided()
        {
            // Arrange
            User existingUser = MockDataFactory.Users.CreateValid(1, "testuser", isAdmin: true);
            EditUser editUser = new(existingUser)
            {
                FirstName = "Updated"
            };

            _viewModel.EditUserModel = editUser;
            _viewModel.Users = [existingUser];

            User updatedUser = MockDataFactory.Users.CreateValid(1, "testuser");
            _mockUsersMediator.Setup(m => m.Update(editUser)).ReturnsAsync(updatedUser);
            _mockUsersMediator.Setup(m => m.GetAllUsers()).ReturnsAsync([updatedUser]);

            // Act
            bool result = await _viewModel.UpdateUser();

            // Assert
            result.Should().BeTrue();
            _viewModel.ShowEditModal.Should().BeFalse();
            _mockUsersMediator.Verify(m => m.Update(editUser), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnFalse_WhenEditUserModelIsNull()
        {
            // Arrange
            _viewModel.EditUserModel = null;

            // Act
            var result = await _viewModel.UpdateUser();

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region UpdateUser Tests - Validation

        [Fact]
        public async Task UpdateUser_ShouldReturnFalse_WhenUsernameAlreadyExists()
        {
            // Arrange
            var user1 = MockDataFactory.Users.CreateValid(1, "user1");
            var user2 = MockDataFactory.Users.CreateValid(2, "user2");
            var editUser = new EditUser(user1);
            editUser.Username = "user2";

            _viewModel.EditUserModel = editUser;
            _viewModel.Users = new List<IUser> { user1, user2 };

            // Act
            var result = await _viewModel.UpdateUser();

            // Assert
            result.Should().BeFalse();
            _viewModel.ValidationErrors.Should().Contain("A user with that username already exists.");
        }

        [Fact]
        public async Task UpdateUser_ShouldPreventRemovingOwnAdminStatus()
        {
            // Arrange
            var currentAdmin = MockDataFactory.Users.CreateValid(1, "admin", isAdmin: true);
            _mockAuthService.Setup(a => a.CurrentUser).Returns(currentAdmin);

            var editUser = new EditUser(currentAdmin);
            editUser.IsAdmin = false;

            _viewModel.EditUserModel = editUser;
            _viewModel.Users = new List<IUser> { currentAdmin };

            // Act
            var result = await _viewModel.UpdateUser();

            // Assert
            result.Should().BeFalse();
            _viewModel.ValidationErrors.Should().Contain("You cannot remove your own admin privileges.");
        }

        [Fact]
        public async Task UpdateUser_ShouldAllowRemovingAnotherUserAdminStatus()
        {
            // Arrange
            var currentAdmin = MockDataFactory.Users.CreateValid(1, "admin", isAdmin: true);
            var otherUser = MockDataFactory.Users.CreateValid(2, "otheruser", isAdmin: true);
            _mockAuthService.Setup(a => a.CurrentUser).Returns(currentAdmin);

            var editUser = new EditUser(otherUser);
            editUser.IsAdmin = false;

            _viewModel.EditUserModel = editUser;
            _viewModel.Users = new List<IUser> { currentAdmin, otherUser };

            _mockUsersMediator.Setup(m => m.Update(editUser)).ReturnsAsync(otherUser);
            _mockUsersMediator.Setup(m => m.GetAllUsers()).ReturnsAsync(new List<IUser> { currentAdmin, otherUser });

            // Act
            var result = await _viewModel.UpdateUser();

            // Assert
            result.Should().BeTrue();
        }

        #endregion

        #region DeleteUser Tests - Happy Path

        [Fact]
        public async Task DeleteUser_ShouldDeleteUser_WhenValidUserSelected()
        {
            // Arrange
            var userToDelete = MockDataFactory.Users.CreateValid(2, "usertoDelete");
            _viewModel.SelectedUser = userToDelete;
            _viewModel.Users = new List<IUser> { _mockAuthService.Object.CurrentUser as IUser, userToDelete };

            _mockUsersMediator.Setup(m => m.Delete(userToDelete.Id)).Returns(Task.CompletedTask);
            _mockUsersMediator.Setup(m => m.GetAllUsers()).ReturnsAsync(new List<IUser> { _mockAuthService.Object.CurrentUser as IUser });

            // Act
            var result = await _viewModel.DeleteUser();

            // Assert
            result.Should().BeTrue();
            _viewModel.ShowDeleteConfirm.Should().BeFalse();
            _mockUsersMediator.Verify(m => m.Delete(userToDelete.Id), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnFalse_WhenNoUserSelected()
        {
            // Arrange
            _viewModel.SelectedUser = null;

            // Act
            var result = await _viewModel.DeleteUser();

            // Assert
            result.Should().BeFalse();
        }

        #endregion

        #region DeleteUser Tests - Validation

        [Fact]
        public async Task DeleteUser_ShouldPreventDeletingOwnAccount()
        {
            // Arrange
            var currentAdmin = MockDataFactory.Users.CreateValid(1, "admin", isAdmin: true);
            _mockAuthService.Setup(a => a.CurrentUser).Returns(currentAdmin);
            _viewModel.SelectedUser = currentAdmin;

            // Act
            var result = await _viewModel.DeleteUser();

            // Assert
            result.Should().BeFalse();
            _viewModel.ErrorMessage.Should().Contain("You cannot delete your own account.");
        }

        [Fact]
        public async Task DeleteUser_ShouldAllowDeletingAnotherUser()
        {
            // Arrange
            var currentAdmin = MockDataFactory.Users.CreateValid(1, "admin", isAdmin: true);
            var otherUser = MockDataFactory.Users.CreateValid(2, "otheruser");
            _mockAuthService.Setup(a => a.CurrentUser).Returns(currentAdmin);
            _viewModel.SelectedUser = otherUser;
            _viewModel.Users = new List<IUser> { currentAdmin, otherUser };

            _mockUsersMediator.Setup(m => m.Delete(otherUser.Id)).Returns(Task.CompletedTask);
            _mockUsersMediator.Setup(m => m.GetAllUsers()).ReturnsAsync(new List<IUser> { currentAdmin });

            // Act
            var result = await _viewModel.DeleteUser();

            // Assert
            result.Should().BeTrue();
            _mockUsersMediator.Verify(m => m.Delete(otherUser.Id), Times.Once);
        }

        #endregion

        #region DeleteConfirm Modal Tests

        [Fact]
        public void OpenDeleteConfirm_ShouldShowModal()
        {
            // Arrange
            var user = MockDataFactory.Users.CreateValid(1, "testuser");

            // Act
            _viewModel.OpenDeleteConfirm(user);

            // Assert
            _viewModel.ShowDeleteConfirm.Should().BeTrue();
            _viewModel.SelectedUser.Should().Be(user);
            _viewModel.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void CloseDeleteConfirm_ShouldHideModal()
        {
            // Arrange
            _viewModel.OpenDeleteConfirm(MockDataFactory.Users.CreateValid(1, "testuser"));

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
            var viewModel = new UsersViewModel(_mockUsersMediator.Object, _mockAuthService.Object);

            // Act
            var role = viewModel.CurrentUserRole;

            // Assert
            role.Should().Be(UserRole.Admin);
        }

        [Fact]
        public void CurrentUserRole_ShouldReturnEditorRole_WhenCurrentUserIsEditor()
        {
            // Arrange
            var editorUser = MockDataFactory.Users.CreateValid(1, "editor", isEditor: true);
            _mockAuthService.Setup(a => a.CurrentUser).Returns(editorUser);
            var viewModel = new UsersViewModel(_mockUsersMediator.Object, _mockAuthService.Object);

            // Act
            var role = viewModel.CurrentUserRole;

            // Assert
            role.Should().Be(UserRole.Editor);
        }

        [Fact]
        public void CurrentUserRole_ShouldReturnStandardRole_WhenCurrentUserIsStandard()
        {
            // Arrange
            var standardUser = MockDataFactory.Users.CreateValid(1, "standard");
            _mockAuthService.Setup(a => a.CurrentUser).Returns(standardUser);
            var viewModel = new UsersViewModel(_mockUsersMediator.Object, _mockAuthService.Object);

            // Act
            var role = viewModel.CurrentUserRole;

            // Assert
            role.Should().Be(UserRole.Standard);
        }

        [Fact]
        public void CurrentUserRole_ShouldReturnStandardRole_WhenNoCurrentUser()
        {
            // Arrange
            _mockAuthService.Setup(a => a.CurrentUser).Returns((IUser?)null);
            var viewModel = new UsersViewModel(_mockUsersMediator.Object, _mockAuthService.Object);

            // Act
            var role = viewModel.CurrentUserRole;

            // Assert
            role.Should().Be(UserRole.Standard);
        }

        #endregion

        #region CurrentUserId Tests

        [Fact]
        public void CurrentUserId_ShouldReturnCurrentUserId()
        {
            // Arrange
            var user = MockDataFactory.Users.CreateValid(5, "testuser");
            _mockAuthService.Setup(a => a.CurrentUser).Returns(user);
            var viewModel = new UsersViewModel(_mockUsersMediator.Object, _mockAuthService.Object);

            // Act
            var userId = viewModel.CurrentUserId;

            // Assert
            userId.Should().Be(5);
        }

        [Fact]
        public void CurrentUserId_ShouldReturnZero_WhenNoCurrentUser()
        {
            // Arrange
            _mockAuthService.Setup(a => a.CurrentUser).Returns((IUser?)null);
            var viewModel = new UsersViewModel(_mockUsersMediator.Object, _mockAuthService.Object);

            // Act
            var userId = viewModel.CurrentUserId;

            // Assert
            userId.Should().Be(0);
        }

        #endregion
    }
}
