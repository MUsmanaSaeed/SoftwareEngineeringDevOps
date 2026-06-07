using SoftwareEngineeringDevOps.App.Bricks;
using SoftwareEngineeringDevOps.App.BrickOrders;
using SoftwareEngineeringDevOps.App.Manufacturers;
using SoftwareEngineeringDevOps.App.Users;

namespace SoftwareEngineeringDevOps.Tests.TestUtilities
{
    /// <summary>
    /// Factory for creating test data objects with predefined values
    /// </summary>
    public static class MockDataFactory
    {
        public static class Manufacturers
        {
            public static NewManufacturer CreateValidNew(string name = "Test Manufacturer")
            {
                return new NewManufacturer
                {
                    Name = name,
                    Address1 = "123 Test Street",
                    Address2 = "Suite 100",
                    Postcode = "SW1A 1AA",
                    PhoneNo = "+441234567890",
                    Email = "test@example.com"
                };
            }

            public static Manufacturer CreateValid(long id = 1, string name = "Test Manufacturer")
            {
                return Manufacturer.FromDBO(new SoftwareEngineeringDevOps.App.Manufacturers.Persistence.DBOs.ManufacturerDBO
                {
                    Id = id,
                    Name = name,
                    Address1 = "123 Test Street",
                    Address2 = "Suite 100",
                    Postcode = "SW1A 1AA",
                    PhoneNo = "+441234567890",
                    Email = "test@example.com"
                });
            }

            public static Manufacturer CreateTestManufacturer() => CreateValid(1, "ACME Bricks Ltd");
        }

        public static class Bricks
        {
            public static NewBrick CreateValidNew(long manufacturerId = 1, string name = "Test Brick")
            {
                return new NewBrick
                {
                    Name = name,
                    ManufacturerId = manufacturerId,
                    Price = 10.50m,
                    Colour = "Red",
                    Material = "Clay",
                    Type = "Standard",
                    Strength = 25.5m,
                    Width = 215m,
                    Height = 102.5m,
                    Depth = 65m,
                    Voids = 0.15m
                };
            }

            public static NewBrick CreateValidNewWithMinimumValues(long manufacturerId = 1)
            {
                return new NewBrick
                {
                    Name = "Min Brick",
                    ManufacturerId = manufacturerId,
                    Price = 0m,
                    Colour = "",
                    Material = "",
                    Type = "",
                    Strength = 0m,
                    Width = 0m,
                    Height = 0m,
                    Depth = 0m,
                    Voids = 0m
                };
            }

            public static NewBrick CreateValidNewWithMaximumValues(long manufacturerId = 1)
            {
                return new NewBrick
                {
                    Name = "Max Brick",
                    ManufacturerId = manufacturerId,
                    Price = 9999.99m,
                    Colour = "Multi",
                    Material = "Concrete",
                    Type = "Engineering",
                    Strength = 999.99m,
                    Width = 10000m,
                    Height = 10000m,
                    Depth = 10000m,
                    Voids = 1m
                };
            }

            public static Brick CreateValid(long id = 1, long manufacturerId = 1, string name = "Test Brick")
            {
                var manufacturer = Manufacturers.CreateValid(manufacturerId, "Test Manufacturer");
                return Brick.FromDBO(new SoftwareEngineeringDevOps.App.Bricks.Persistence.DBOs.BrickDBO
                {
                    Id = id,
                    Name = name,
                    ManufacturerId = manufacturerId,
                    Price = 10.50m,
                    Colour = "Red",
                    Material = "Clay",
                    Type = "Standard",
                    Strength = 25.5m,
                    Width = 215m,
                    Height = 102.5m,
                    Depth = 65m,
                    Voids = 0.15m
                }, manufacturer);
            }
        }

        public static class BrickOrders
        {
            public static NewBrickOrder CreateValidNew(long brickId = 1, int quantity = 1000)
            {
                return new NewBrickOrder
                {
                    BrickId = brickId,
                    OrderNo = $"ORD-{DateTime.UtcNow:yyyyMMdd}-001",
                    BricksOrdered = quantity,
                    OrderedDate = DateTime.UtcNow
                };
            }

            public static BrickOrder CreateValid(long id = 1, long brickId = 1, string orderNo = "ORD-20260101-001", bool isCancelled = false)
            {
                var createdByUser = Users.CreateValid(1);
                return BrickOrder.FromDBO(new SoftwareEngineeringDevOps.App.BrickOrders.Persistence.DBOs.BrickOrderDBO
                {
                    Id = id,
                    OrderNo = orderNo,
                    BrickId = brickId,
                    BricksOrdered = 1000,
                    OrderedDate = DateTime.UtcNow,
                    ExpectedDate = DateTime.UtcNow.AddDays(30),
                    CancelledDate = isCancelled ? DateTime.UtcNow : null
                }, createdByUser);
            }
        }

        public static class Users
        {
            public static NewUser CreateValidNew(string username = "testuser", bool isAdmin = false, bool isEditor = false)
            {
                return new NewUser
                {
                    Username = username,
                    Password = "SecurePassword123!",
                    FirstName = "Test",
                    LastName = "User",
                    IsAdmin = isAdmin,
                    IsEditor = isEditor
                };
            }

            public static User CreateValid(long id = 1, string username = "testuser", bool isAdmin = false, bool isEditor = false)
            {
                return User.FromDBO(new SoftwareEngineeringDevOps.App.Users.Persistence.DBOs.UserDBO
                {
                    Id = id,
                    Username = username,
                    Password = "SecurePassword123!",
                    FirstName = "Test",
                    LastName = "User",
                    IsAdmin = isAdmin,
                    IsEditor = isEditor
                });
            }

            public static User CreateAdmin() => CreateValid(1, "admin", isAdmin: true, isEditor: false);
            public static User CreateEditor() => CreateValid(2, "editor", isAdmin: false, isEditor: true);
            public static User CreateStandardUser() => CreateValid(3, "standard", isAdmin: false, isEditor: false);
        }
    }
}
