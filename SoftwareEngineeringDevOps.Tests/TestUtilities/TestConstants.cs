namespace SoftwareEngineeringDevOps.Tests.TestUtilities
{
    /// <summary>
    /// Centralized constants for test scenarios
    /// </summary>
    public static class TestConstants
    {
        public const string ValidUsername = "testuser";
        public const string ValidPassword = "SecurePassword123!";
        public const string InvalidUsername = "nonexistent";
        public const string InvalidPassword = "wrongpassword";
        
        public const string EmptyString = "";
        public const string WhitespaceString = "   ";
        
        public const decimal MaxDimensionLimit = 10000m;
        public const decimal MinDecimalValue = 0m;
        public const decimal MaxVoidsPercent = 1m;
        
        public const int DefaultQuantity = 1000;
        public const int MinQuantity = 1;
        public const int MaxQuantity = 999999;
        
        public static class ErrorMessages
        {
            public const string DatabaseConnectionFailed = "Database connection failed";
            public const string UnauthorizedAccess = "Unauthorized access";
            public const string InvalidInput = "Invalid input";
            public const string NotFound = "Resource not found";
        }
    }
}
