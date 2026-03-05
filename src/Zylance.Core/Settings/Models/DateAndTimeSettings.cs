using System.Globalization;

namespace Zylance.Core.Settings.Models;

/// <summary>
///     Settings for date and time display formatting, including format patterns
///     and timezone preferences.
/// </summary>
public record DateAndTimeSettings
{
    /// <summary>
    ///     The format pattern used for displaying dates.
    ///     NOTE: use DatePattern to get the actual pattern, which handles the
    ///     "system" default case.
    /// </summary>
    public string DateFormat { get; init; } = "system";

    /// <summary>
    ///     The format pattern used for displaying times.
    ///     NOTE: use TimePattern to get the actual pattern, which handles the
    ///     "system" default case.
    /// </summary>
    public string TimeFormat { get; init; } = "system";

    private static DateTimeFormatInfo SystemTimeInfo => CultureInfo.CurrentCulture.DateTimeFormat;

    public string DatePattern => DateFormat == "system" ? SystemTimeInfo.ShortDatePattern : DateFormat;

    public string TimePattern => TimeFormat == "system" ? SystemTimeInfo.ShortTimePattern : TimeFormat;
}
