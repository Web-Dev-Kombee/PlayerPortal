namespace Shard.Commons
{
    public static class DateExtensions
    {
        public static DateTimeOffset? FormatDate(this string date)
        {
            return date.ToDateTimeOffset(System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AllowWhiteSpaces);
        }
    }
}
