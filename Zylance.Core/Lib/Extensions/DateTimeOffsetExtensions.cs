namespace Zylance.Core.Lib.Extensions;

/// <summary>
///     Extension methods for DateTimeOffset
/// </summary>
public static class DateTimeOffsetExtensions
{
    extension(DateTimeOffset dateTime)
    {
        public string ToIso8601()
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffK");
        }
    }
}
