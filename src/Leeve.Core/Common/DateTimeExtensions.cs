namespace Leeve.Core.Common;

public static class DateTimeExtensions
{
    public static DateTime ToLocalDateTime(this DateTime utcDateTime) =>
        TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.Local);
}