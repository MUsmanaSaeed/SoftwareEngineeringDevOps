using SoftwareEngineeringDevOps.App.Users;
using System.Text.RegularExpressions;

namespace SoftwareEngineeringDevOps.App.Validation
{
    public static partial class InputValidator
    {
        static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static readonly Regex PhoneRegex = new(
            @"^\+?\d{7,20}$",
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

        // Each string represents a physical row on a standard QWERTY keyboard (left-to-right),
        // used to detect keyboard-walk patterns algorithmically.
        static readonly string[] KeyboardRows = ["qwertyuiop", "asdfghjkl", "zxcvbnm", "1234567890"];

        // Leet-speak substitution map used to normalise passwords before pattern checks,
        // so obfuscated variants like "p@ssw0rd" are caught by the same logic as "password".
        static readonly Dictionary<char, char> LeetMap = new()
        {
            ['@'] = 'a', ['4'] = 'a',
            ['3'] = 'e',
            ['1'] = 'i', ['!'] = 'i',
            ['0'] = 'o',
            ['5'] = 's', ['$'] = 's',
            ['7'] = 't', ['+'] = 't',
            ['2'] = 'z',
        };

        // High-frequency base words extracted from real-world credential breach analyses.
        // Used by ContainsBaseWord to detect word+padding patterns (e.g. "password1234",
        // "1234admin!", "p@ssw0rd99") after leet normalisation has been applied.
        static readonly HashSet<string> CommonBaseWords = new(StringComparer.Ordinal)
        {
            // Authentication / system words
            "password", "passphrase", "passwd", "admin", "administrator",
            "login", "welcome", "letmein", "access", "changeme",
            "default", "guest", "root", "master", "secret",
            "user", "test", "demo", "temp", "service",
        };

        // Matches contiguous runs of four or more lowercase letters within a string.
        // Used to extract the letter core of a password after leet normalisation,
        // isolating the meaningful word regardless of surrounding digits/symbols.
        static readonly Regex LetterRunRegex = new(@"[a-z]{4,}", RegexOptions.Compiled);

        static string NormalizeLeet(string value)
        {
            var chars = value.ToLowerInvariant().ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (LeetMap.TryGetValue(chars[i], out var plain))
                    chars[i] = plain;
            }
            return new string(chars);
        }

        // Returns true when every character in the password is identical (e.g. "aaaaaaaaaaaa").
        static bool IsRepeatedSingleChar(string password)
            => password.Distinct().Count() == 1;

        // Returns true when more than half the password is a consecutive ascending or descending
        // character/digit run (e.g. "123456789012", "abcdefghijkl", "zyxwvutsrqpo").
        static bool IsSequentialRun(string password)
        {
            int asc = 1, desc = 1, maxRun = 1;
            for (var i = 1; i < password.Length; i++)
            {
                asc  = password[i] == password[i - 1] + 1 ? asc  + 1 : 1;
                desc = password[i] == password[i - 1] - 1 ? desc + 1 : 1;
                maxRun = Math.Max(maxRun, Math.Max(asc, desc));
            }
            return maxRun > password.Length / 2;
        }

        // Returns true when any 5+ character contiguous keyboard-row slice (forward or reversed)
        // appears in the password (e.g. "qwerty", "asdfgh", "ytrewq").
        static bool IsKeyboardWalk(string password, int minLength = 5)
        {
            var lower = password.ToLowerInvariant();
            foreach (var row in KeyboardRows)
            {
                var reversed = new string(row.Reverse().ToArray());
                for (var len = minLength; len <= row.Length; len++)
                {
                    for (var start = 0; start <= row.Length - len; start++)
                    {
                        var slice = row.Substring(start, len);
                        if (lower.Contains(slice) || lower.Contains(reversed.Substring(row.Length - start - len, len)))
                            return true;
                    }
                }
            }
            return false;
        }

        // Returns true when the ratio of unique characters to total length is below 30 %,
        // indicating a highly repetitive password (e.g. "aabababababa").
        static bool HasLowCharacterVariety(string password)
            => password.Length >= 12 && (double)password.Distinct().Count() / password.Length < 0.3;

        // Returns true when the password can be fully expressed as a shorter chunk repeated
        // two or more times (e.g. "abcabcabcabc", "xoxoxoxoxoxo").
        static bool HasRepeatingChunk(string password)
        {
            var len = password.Length;
            for (var chunkLen = 2; chunkLen <= len / 2; chunkLen++)
            {
                if (len % chunkLen != 0) continue;
                var chunk = password[..chunkLen];
                if (Enumerable.Range(0, len / chunkLen).All(i => password.Substring(i * chunkLen, chunkLen) == chunk))
                    return true;
            }
            return false;
        }

        // Returns true when any contiguous run of letters in the (leet-normalised) password
        // exactly matches a high-frequency base word, regardless of surrounding digits or
        // symbols. Detects patterns like "password1234", "1234admin!", "p@ssw0rd_99".
        static bool ContainsBaseWord(string normalizedPassword)
            => LetterRunRegex.Matches(normalizedPassword).Any(m => CommonBaseWords.Contains(m.Value));

        /// <summary>
        /// Validates password strength using modern security requirements:
        /// <list type="bullet">
        ///   <item>Minimum 12 characters</item>
        ///   <item>Maximum 128 characters (prevents DoS via expensive hashing)</item>
        ///   <item>Must contain at least one letter, one digit, and one special character</item>
        ///   <item>Spaces are allowed; control characters are not</item>
        ///   <item>Blocks structurally weak patterns checked against both raw and leet-normalised forms:
        ///     repeated characters, sequential runs, keyboard walks, low character variety,
        ///     repeating chunks, and common base words with number/symbol padding</item>
        ///   <item>Must not contain the username, first name, or last name</item>
        /// </list>
        /// </summary>
        public static ValidationResult ValidatePassword(string? password, string fieldName = "Password", string? username = null, string? firstName = null, string? lastName = null)
        {
            var nonEmpty = ValidateNonEmpty(password, fieldName);
            if (!nonEmpty.IsValid) return nonEmpty;

            if (password!.Length < 12)
                return ValidationResult.Error($"{fieldName} must be at least 12 characters long.");

            if (password.Length > 128)
                return ValidationResult.Error($"{fieldName} must not exceed 128 characters.");

            // Spaces are explicitly permitted; only non-printable control characters are rejected.
            if (password.Any(c => char.IsControl(c) && c != ' '))
                return ValidationResult.Error($"{fieldName} must not contain control characters.");

            // Character class requirements — each must be present to ensure meaningful complexity.
            if (!password.Any(char.IsLetter))
                return ValidationResult.Error($"{fieldName} must contain at least one letter.");

            if (!password.Any(char.IsDigit))
                return ValidationResult.Error($"{fieldName} must contain at least one number.");

            if (!password.Any(c => !char.IsLetterOrDigit(c) && !char.IsControl(c)))
                return ValidationResult.Error($"{fieldName} must contain at least one special character (e.g. !, @, #, $).");

            // All pattern checks run against both the raw password and its leet-normalised form
            // to catch obfuscated variants (e.g. "p@ssw0rd1234", "qu3rtyuiop!!", "4dm1n99").
            var normalized = NormalizeLeet(password);
            foreach (var candidate in new[] { password.ToLowerInvariant(), normalized })
            {
                if (IsRepeatedSingleChar(candidate))
                    return ValidationResult.Error($"{fieldName} must not consist of a single repeated character.");

                if (IsSequentialRun(candidate))
                    return ValidationResult.Error($"{fieldName} must not be a simple sequential pattern (e.g. \"123456789012\" or \"abcdefghijkl\").");

                if (IsKeyboardWalk(candidate))
                    return ValidationResult.Error($"{fieldName} must not follow a keyboard pattern (e.g. \"qwerty\" or \"asdfgh\").");

                if (HasLowCharacterVariety(candidate))
                    return ValidationResult.Error($"{fieldName} is too repetitive. Please use a more varied password.");

                if (HasRepeatingChunk(candidate))
                    return ValidationResult.Error($"{fieldName} must not be a repeated pattern (e.g. \"abcabcabcabc\").");

                if (ContainsBaseWord(candidate))
                    return ValidationResult.Error($"{fieldName} must not be based on a common word (e.g. \"password\", \"admin\") with numbers or symbols appended.");
            }

            // Block passwords that contain the username, first name, or last name,
            // including leet-obfuscated forms of each.
            var contextWords = new[] { username, firstName, lastName };
            foreach (var word in contextWords)
            {
                if (string.IsNullOrWhiteSpace(word)) continue;

                if (password.Contains(word, StringComparison.OrdinalIgnoreCase)
                    || normalized.Contains(NormalizeLeet(word), StringComparison.OrdinalIgnoreCase))
                    return ValidationResult.Error($"{fieldName} must not contain your username, first name, or last name.");
            }

            return ValidationResult.Success();
        }

        public static List<string> ValidateManufacturer(Manufacturers.NewManufacturer m)
        {
            var errors = new List<string>();
            AddIfInvalid(errors, ValidateNonEmpty(m.Name, "Name"));
            AddIfInvalid(errors, ValidateNonEmpty(m.Address1, "Address Line 1"));
            AddIfInvalid(errors, ValidatePhone(m.PhoneNo, "Phone Number"));
            AddIfInvalid(errors, ValidateEmail(m.Email, "Email"));
            return errors;
        }

        public static List<string> ValidateManufacturer(Manufacturers.EditManufacturer m)
        {
            var errors = new List<string>();
            AddIfInvalid(errors, ValidateNonEmpty(m.Name, "Name"));
            AddIfInvalid(errors, ValidateNonEmpty(m.Address1, "Address Line 1"));
            AddIfInvalid(errors, ValidatePhone(m.PhoneNo, "Phone Number"));
            AddIfInvalid(errors, ValidateEmail(m.Email, "Email"));
            return errors;
        }

        public static List<string> ValidateUser(NewUser u, bool requireLastName = true, bool requireStrongPassword = true)
        {
            var errors = new List<string>();
            AddIfInvalid(errors, ValidateNonEmpty(u.Username, "Username"));
            if (requireStrongPassword)
                AddIfInvalid(errors, ValidatePassword(u.Password, "Password", u.Username, u.FirstName, u.LastName));
            else
                AddIfInvalid(errors, ValidateNonEmpty(u.Password, "Password"));
            AddIfInvalid(errors, ValidateNonEmpty(u.FirstName, "First Name"));
            if (requireLastName) AddIfInvalid(errors, ValidateNonEmpty(u.LastName, "Last Name"));
            return errors;
        }

        public static List<string> ValidateUser(EditUser u, bool requireLastName = true, bool requireStrongPassword = true)
        {
            var errors = new List<string>();
            AddIfInvalid(errors, ValidateNonEmpty(u.Username, "Username"));
            // Password is optional on edit; only validate if a new one is provided
            if (!string.IsNullOrWhiteSpace(u.Password))
            {
                if (requireStrongPassword)
                    AddIfInvalid(errors, ValidatePassword(u.Password, "Password", u.Username, u.FirstName, u.LastName));
            }
            AddIfInvalid(errors, ValidateNonEmpty(u.FirstName, "First Name"));
            if (requireLastName) AddIfInvalid(errors, ValidateNonEmpty(u.LastName, "Last Name"));
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

        static void AddIfInvalid(List<string> errors, ValidationResult result)
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
