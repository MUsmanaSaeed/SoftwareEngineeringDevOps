using System.Text.RegularExpressions;

namespace SoftwareEngineeringDevOps.App.Validation
{
    public static partial class InputValidator
    {
        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PhoneRegex = new(
            @"^[\+]?[\d\s\-\(\)]{7,20}$",
            RegexOptions.Compiled);

        public static ValidationResult ValidateNonEmpty(string? value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ValidationResult.Error($"{fieldName} is required and cannot be empty.");
            return ValidationResult.Success();
        }

        public static ValidationResult ValidateEmail(string? email, string fieldName = "Email")
        {
            var nonEmpty = ValidateNonEmpty(email, fieldName);
            if (!nonEmpty.IsValid) return nonEmpty;

            if (!EmailRegex.IsMatch(email!))
                return ValidationResult.Error($"{fieldName} must be a valid email address.");
            return ValidationResult.Success();
        }

        public static ValidationResult ValidatePhone(string? phone, string fieldName = "Phone")
        {
            var nonEmpty = ValidateNonEmpty(phone, fieldName);
            if (!nonEmpty.IsValid) return nonEmpty;

            if (!PhoneRegex.IsMatch(phone!))
                return ValidationResult.Error($"{fieldName} must be a valid phone number.");
            return ValidationResult.Success();
        }

        public static List<string> ValidateManufacturer(Manufacturers.NewManufacturer m)
        {
            var errors = new List<string>();
            AddIfInvalid(errors, ValidateNonEmpty(m.Name, "Name"));
            AddIfInvalid(errors, ValidateNonEmpty(m.Address1, "Address1"));
            AddIfInvalid(errors, ValidateNonEmpty(m.Postcode, "Postcode"));
            AddIfInvalid(errors, ValidatePhone(m.PhoneNo, "Phone Number"));
            AddIfInvalid(errors, ValidateEmail(m.Email, "Email"));
            return errors;
        }

        public static List<string> ValidateManufacturer(Manufacturers.EditManufacturer m)
        {
            var errors = new List<string>();
            AddIfInvalid(errors, ValidateNonEmpty(m.Name, "Name"));
            AddIfInvalid(errors, ValidateNonEmpty(m.Address1, "Address1"));
            AddIfInvalid(errors, ValidateNonEmpty(m.Postcode, "Postcode"));
            AddIfInvalid(errors, ValidatePhone(m.PhoneNo, "Phone Number"));
            AddIfInvalid(errors, ValidateEmail(m.Email, "Email"));
            return errors;
        }

        public static List<string> ValidateUser(Users.NewUser u)
        {
            var errors = new List<string>();
            AddIfInvalid(errors, ValidateNonEmpty(u.Username, "Username"));
            AddIfInvalid(errors, ValidateNonEmpty(u.Password, "Password"));
            AddIfInvalid(errors, ValidateNonEmpty(u.FirstName, "First Name"));
            AddIfInvalid(errors, ValidateNonEmpty(u.LastName, "Last Name"));
            return errors;
        }

        public static List<string> ValidateUser(Users.EditUser u)
        {
            var errors = new List<string>();
            AddIfInvalid(errors, ValidateNonEmpty(u.Username, "Username"));
            AddIfInvalid(errors, ValidateNonEmpty(u.Password, "Password"));
            AddIfInvalid(errors, ValidateNonEmpty(u.FirstName, "First Name"));
            AddIfInvalid(errors, ValidateNonEmpty(u.LastName, "Last Name"));
            return errors;
        }

        public static List<string> ValidateBrick(Bricks.NewBrick b)
        {
            var errors = new List<string>();
            AddIfInvalid(errors, ValidateNonEmpty(b.Name, "Name"));
            AddIfInvalid(errors, ValidateNonEmpty(b.Colour, "Colour"));
            AddIfInvalid(errors, ValidateNonEmpty(b.Material, "Material"));
            AddIfInvalid(errors, ValidateNonEmpty(b.Type, "Type"));
            if (b.ManufacturerId <= 0) errors.Add("Manufacturer is required.");
            return errors;
        }

        public static List<string> ValidateBrick(Bricks.EditBrick b)
        {
            var errors = new List<string>();
            AddIfInvalid(errors, ValidateNonEmpty(b.Name, "Name"));
            AddIfInvalid(errors, ValidateNonEmpty(b.Colour, "Colour"));
            AddIfInvalid(errors, ValidateNonEmpty(b.Material, "Material"));
            AddIfInvalid(errors, ValidateNonEmpty(b.Type, "Type"));
            if (b.ManufacturerId <= 0) errors.Add("Manufacturer is required.");
            return errors;
        }

        public static List<string> ValidateBrickOrder(BrickOrders.NewBrickOrder o)
        {
            var errors = new List<string>();
            AddIfInvalid(errors, ValidateNonEmpty(o.OrderNo, "Order Number"));
            if (o.BrickId <= 0) errors.Add("Brick is required.");
            if (o.BricksOrdered <= 0) errors.Add("Bricks ordered must be greater than zero.");
            return errors;
        }

        public static List<string> ValidateBrickOrder(BrickOrders.EditBrickOrder o)
        {
            var errors = new List<string>();
            if (o.BrickId <= 0) errors.Add("Brick is required.");
            if (o.BricksOrdered <= 0) errors.Add("Bricks ordered must be greater than zero.");
            return errors;
        }

        public static List<string> ValidateBrickOrderReceived(BrickOrdersReceived.NewBrickOrderReceived r)
        {
            var errors = new List<string>();
            if (r.BrickOrderId <= 0) errors.Add("Brick Order is required.");
            if (r.BricksReceived <= 0) errors.Add("Bricks received must be greater than zero.");
            return errors;
        }

        public static List<string> ValidateBrickOrderReceived(BrickOrdersReceived.EditBrickOrderReceived r)
        {
            var errors = new List<string>();
            if (r.BricksReceived <= 0) errors.Add("Bricks received must be greater than zero.");
            return errors;
        }

        private static void AddIfInvalid(List<string> errors, ValidationResult result)
        {
            if (!result.IsValid)
                errors.Add(result.ErrorMessage!);
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static ValidationResult Success() => new() { IsValid = true };
        public static ValidationResult Error(string message) => new() { IsValid = false, ErrorMessage = message };
    }
}
