using System;

namespace Etalem.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToRelativeTime(this DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "Unknown";

            var timeSpan = DateTime.UtcNow - dateTime.Value;
            if (timeSpan.TotalDays > 30)
                return $"{(int)timeSpan.TotalDays / 30} month(s) ago";
            if (timeSpan.TotalDays > 7)
                return $"{(int)timeSpan.TotalDays / 7} week(s) ago";
            if (timeSpan.TotalDays > 1)
                return $"{(int)timeSpan.TotalDays} day(s) ago";
            if (timeSpan.TotalHours > 1)
                return $"{(int)timeSpan.TotalHours} hour(s) ago";
            if (timeSpan.TotalMinutes > 1)
                return $"{(int)timeSpan.TotalMinutes} minute(s) ago";
            return "just now";
        }
    }
}