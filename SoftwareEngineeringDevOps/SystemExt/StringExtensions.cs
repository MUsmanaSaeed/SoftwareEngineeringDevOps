namespace System
{
    public static class StringExtensions
    {
        public static string ToTitleCase(this string? value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            var chars = value.ToCharArray();
            bool capitalizeNext = true;
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] == ' ')
                {
                    capitalizeNext = true;
                }
                else if (capitalizeNext)
                {
                    chars[i] = char.ToUpperInvariant(chars[i]);
                    capitalizeNext = false;
                }
                else
                {
                    chars[i] = char.ToLowerInvariant(chars[i]);
                }
            }
            return new string(chars);
        }
    }
}
