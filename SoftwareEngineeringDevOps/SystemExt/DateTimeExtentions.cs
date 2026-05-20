namespace System
{
    public static class DateTimeExtentions
    {
        private static readonly TimeZoneInfo UkTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        public static DateTime ToUkTime(this DateTime dateTime)
        {
            DateTime utcTime = dateTime.Kind == DateTimeKind.Utc
                ? dateTime
                : dateTime.ToUniversalTime();

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, UkTimeZone);
        }
    }
}
