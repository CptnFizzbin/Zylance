namespace Zylance.Core.Lib.Extensions;

/// <summary>
/// Extension methods for DateTimeOffset
/// </summary>
public static class DateTimeOffsetExtensions
{
    /// <summary>
    /// Converts the DateTimeOffset to an ISO 8601 formatted string using the round-trip format specifier ('o').
    /// This produces a string in the format: yyyy-MM-ddTHH:mm:ss.fffffffzzz
    /// </summary>
    /// <param name="dateTimeOffset">The DateTimeOffset to format</param>
    /// <returns>ISO 8601 formatted timestamp string</returns>
    public static string ToIsoTimestamp(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("o");
    }
}
