using FluentAssertions;
using SoftwareEngineeringDevOps.App.Validation;
using SoftwareEngineeringDevOps.Tests.TestUtilities;

namespace SoftwareEngineeringDevOps.Tests.UnitTests.Validation
{
    /// <summary>
    /// Unit tests for InputValidator covering validation rules, edge cases, and boundary conditions
    /// </summary>
    public class InputValidatorTests
    {
        #region Brick Validation Tests

        [Fact]
        public void ValidateBrick_ShouldReturnNoErrors_WhenAllFieldsValid()
        {
            // Arrange
            var validBrick = MockDataFactory.Bricks.CreateValidNew();

            // Act
            var errors = InputValidator.ValidateBrick(validBrick);

            // Assert
            errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateBrick_ShouldReturnError_WhenNameIsNullOrWhitespace(string? invalidName)
        {
            // Arrange
            var brick = MockDataFactory.Bricks.CreateValidNew();
            brick.Name = invalidName!;

            // Act
            var errors = InputValidator.ValidateBrick(brick);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Name") || e.Contains("name"));
        }

        [Fact]
        public void ValidateBrick_ShouldReturnError_WhenManufacturerIdIsZero()
        {
            // Arrange
            var brick = MockDataFactory.Bricks.CreateValidNew();
            brick.ManufacturerId = 0;

            // Act
            var errors = InputValidator.ValidateBrick(brick);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Manufacturer"));
        }

        [Fact]
        public void ValidateBrick_ShouldReturnError_WhenManufacturerIdIsNegative()
        {
            // Arrange
            var brick = MockDataFactory.Bricks.CreateValidNew();
            brick.ManufacturerId = -1;

            // Act
            var errors = InputValidator.ValidateBrick(brick);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Manufacturer"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateBrick_ShouldReturnError_WhenColourIsNullOrWhitespace(string? invalidColour)
        {
            // Arrange
            var brick = MockDataFactory.Bricks.CreateValidNew();
            brick.Colour = invalidColour!;

            // Act
            var errors = InputValidator.ValidateBrick(brick);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Colour"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateBrick_ShouldReturnError_WhenMaterialIsNullOrWhitespace(string? invalidMaterial)
        {
            // Arrange
            var brick = MockDataFactory.Bricks.CreateValidNew();
            brick.Material = invalidMaterial!;

            // Act
            var errors = InputValidator.ValidateBrick(brick);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Material"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateBrick_ShouldReturnError_WhenTypeIsNullOrWhitespace(string? invalidType)
        {
            // Arrange
            var brick = MockDataFactory.Bricks.CreateValidNew();
            brick.Type = invalidType!;

            // Act
            var errors = InputValidator.ValidateBrick(brick);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Type"));
        }

        [Fact]
        public void ValidateBrick_ShouldAcceptMinimumValues()
        {
            // Arrange
            var brick = MockDataFactory.Bricks.CreateValidNewWithMinimumValues();
            brick.Colour = "Red";
            brick.Material = "Clay";
            brick.Type = "Standard";

            // Act
            var errors = InputValidator.ValidateBrick(brick);

            // Assert
            errors.Should().BeEmpty();
        }

        [Fact]
        public void ValidateBrick_ShouldAcceptMaximumValues()
        {
            // Arrange
            var brick = MockDataFactory.Bricks.CreateValidNewWithMaximumValues();

            // Act
            var errors = InputValidator.ValidateBrick(brick);

            // Assert
            errors.Should().BeEmpty();
        }

        #endregion

        #region Manufacturer Validation Tests

        [Fact]
        public void ValidateManufacturer_ShouldReturnNoErrors_WhenAllFieldsValid()
        {
            // Arrange
            var validManufacturer = MockDataFactory.Manufacturers.CreateValidNew();

            // Act
            var errors = InputValidator.ValidateManufacturer(validManufacturer);

            // Assert
            errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateManufacturer_ShouldReturnError_WhenNameIsNullOrWhitespace(string? invalidName)
        {
            // Arrange
            var manufacturer = MockDataFactory.Manufacturers.CreateValidNew();
            manufacturer.Name = invalidName!;

            // Act
            var errors = InputValidator.ValidateManufacturer(manufacturer);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Name") || e.Contains("name"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateManufacturer_ShouldReturnError_WhenAddress1IsNullOrWhitespace(string? invalidAddress)
        {
            // Arrange
            var manufacturer = MockDataFactory.Manufacturers.CreateValidNew();
            manufacturer.Address1 = invalidAddress!;

            // Act
            var errors = InputValidator.ValidateManufacturer(manufacturer);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Address"));
        }

        #endregion

        #region BrickOrder Validation Tests

        [Fact]
        public void ValidateBrickOrder_ShouldReturnNoErrors_WhenAllFieldsValid()
        {
            // Arrange
            var validOrder = MockDataFactory.BrickOrders.CreateValidNew();

            // Act
            var errors = InputValidator.ValidateBrickOrder(validOrder);

            // Assert
            errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateBrickOrder_ShouldReturnError_WhenOrderNoIsNullOrWhitespace(string? invalidOrderNo)
        {
            // Arrange
            var order = MockDataFactory.BrickOrders.CreateValidNew();
            order.OrderNo = invalidOrderNo!;

            // Act
            var errors = InputValidator.ValidateBrickOrder(order);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Order Number"));
        }

        [Fact]
        public void ValidateBrickOrder_ShouldReturnError_WhenBrickIdIsZero()
        {
            // Arrange
            var order = MockDataFactory.BrickOrders.CreateValidNew();
            order.BrickId = 0;

            // Act
            var errors = InputValidator.ValidateBrickOrder(order);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Brick"));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void ValidateBrickOrder_ShouldReturnError_WhenBricksOrderedIsZeroOrNegative(int invalidQuantity)
        {
            // Arrange
            var order = MockDataFactory.BrickOrders.CreateValidNew();
            order.BricksOrdered = invalidQuantity;

            // Act
            var errors = InputValidator.ValidateBrickOrder(order);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("quantity") || e.Contains("Quantity") || e.Contains("ordered"));
        }

        [Fact]
        public void ValidateBrickOrder_ShouldAcceptMinimumQuantity()
        {
            // Arrange
            var order = MockDataFactory.BrickOrders.CreateValidNew();
            order.BricksOrdered = 1;

            // Act
            var errors = InputValidator.ValidateBrickOrder(order);

            // Assert
            errors.Should().BeEmpty();
        }

        #endregion

        #region User Validation Tests

        [Fact]
        public void ValidateUser_ShouldReturnNoErrors_WhenAllFieldsValid()
        {
            // Arrange
            var validUser = MockDataFactory.Users.CreateValidNew();

            // Act
            var errors = InputValidator.ValidateUser(validUser);

            // Assert
            errors.Should().BeEmpty();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateUser_ShouldReturnError_WhenUsernameIsNullOrWhitespace(string? invalidUsername)
        {
            // Arrange
            var user = MockDataFactory.Users.CreateValidNew();
            user.Username = invalidUsername!;

            // Act
            var errors = InputValidator.ValidateUser(user);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Username"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateUser_ShouldReturnError_WhenPasswordIsNullOrWhitespace(string? invalidPassword)
        {
            // Arrange
            var user = MockDataFactory.Users.CreateValidNew();
            user.Password = invalidPassword!;

            // Act
            var errors = InputValidator.ValidateUser(user);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("Password"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidateUser_ShouldReturnError_WhenFirstNameIsNullOrWhitespace(string? invalidFirstName)
        {
            // Arrange
            var user = MockDataFactory.Users.CreateValidNew();
            user.FirstName = invalidFirstName!;

            // Act
            var errors = InputValidator.ValidateUser(user);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Should().Contain(e => e.Contains("First") && e.Contains("Name"));
        }

        [Fact]
        public void ValidateUser_ShouldAllowEmptyLastName()
        {
            // Arrange
            var user = MockDataFactory.Users.CreateValidNew();
            user.LastName = string.Empty;

            // Act
            var errors = InputValidator.ValidateUser(user, requireLastName: false);

            // Assert
            // Last name is not required when requireLastName is false
            errors.Should().BeEmpty();
        }

        #endregion

        #region Multiple Validation Errors Tests

        [Fact]
        public void ValidateBrick_ShouldReturnMultipleErrors_WhenMultipleFieldsInvalid()
        {
            // Arrange
            var invalidBrick = new SoftwareEngineeringDevOps.App.Bricks.NewBrick
            {
                Name = null!,
                ManufacturerId = 0,
                Colour = null!,
                Material = null!,
                Type = null!
            };

            // Act
            var errors = InputValidator.ValidateBrick(invalidBrick);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Count.Should().BeGreaterThan(1);
        }

        [Fact]
        public void ValidateManufacturer_ShouldReturnMultipleErrors_WhenMultipleFieldsInvalid()
        {
            // Arrange
            var invalidManufacturer = new SoftwareEngineeringDevOps.App.Manufacturers.NewManufacturer
            {
                Name = null!,
                Address1 = null!,
                Postcode = null!,
                PhoneNo = null!,
                Email = null!
            };

            // Act
            var errors = InputValidator.ValidateManufacturer(invalidManufacturer);

            // Assert
            errors.Should().NotBeEmpty();
            errors.Count.Should().BeGreaterThan(1);
        }

        #endregion
    }
}
