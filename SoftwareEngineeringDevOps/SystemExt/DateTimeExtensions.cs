namespace System
{
    public static class DateTimeExtensions
    {
        private static readonly TimeZoneInfo UkTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        public static DateTime ToUkTime(this DateTime dateTime)
        {
            DateTime utcTime = dateTime.Kind == DateTimeKind.Utc
                ? dateTime
                : dateTime.ToUniversalTime();

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, UkTimeZone);
        }

        public static DateTime ToUtc(this DateTime dateTime)
        {
            if (dateTime.Kind == DateTimeKind.Utc)
                return dateTime;
            return dateTime.ToUniversalTime();
        }
    }
}
