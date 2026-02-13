namespace Zylance.Core.Lib.Extensions;

/// <summary>
///     Extension methods for DateTimeOffset
/// </summary>
public static class DateTimeOffsetExtensions
{
    extension(DateTimeOffset dateTime)
    {
        /// <summary>
        /// Converts the DateTimeOffset to an ISO 8601 formatted string with milliseconds and offset.
        /// </summary>
        public string ToIso8601()
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK");
        }
    }
}
