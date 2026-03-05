using System.Globalization;

namespace Zylance.Core.Settings.Models;

/// <summary>
///     Settings for date and time display formatting, including format patterns
///     and timezone preferences.
/// </summary>
public record DateAndTimeSettings
{
    /// <summary>
    ///     Available date format patterns that users can choose from.
    ///     Includes ISO format (yyyy-MM-dd), US format (MM/dd/yyyy), European format
    ///     (dd/MM/yyyy),
    ///     and textual formats like "May 24, 2026".
    /// </summary>
    public static readonly List<string> DateFormats =
    [
        "yyyy-MM-dd",
        // "05/24/2026",
        "MM/dd/yyyy",
        // "24/05/2026"
        "dd/MM/yyyy",
        // "May 24, 2026"
        "MMM d, yyyy",
        // "24 May 2026"
        "d MMM yyyy",
    ];

    /// <summary>
    ///     Available time format patterns that users can choose from.
    ///     Includes 12-hour formats with AM/PM and 24-hour formats, with optional
    ///     seconds display.
    /// </summary>
    public static readonly List<string> TimeFormats =
    [
        "h:mm tt",
        // "22:30",
        "HH:mm",
        // "10:30:45 PM",
        "h:mm:ss tt",
        // "22:30:45",
        "HH:mm:ss",
    ];

    /// <summary>
    ///     Gets the format pattern used for displaying dates.
    ///     Defaults to the system culture's short date pattern if it's in
    ///     <see cref="DateFormats" />,
    ///     otherwise defaults to "yyyy-MM-dd".
    /// </summary>
    public string DateFormat { get; init; } = GetDefaultDateFormat();

    /// <summary>
    ///     Gets the format pattern used for displaying times.
    ///     Defaults to 12-hour format (h:mm tt) if the system culture uses AM/PM,
    ///     otherwise defaults to 24-hour format (HH:mm).
    /// </summary>
    public string TimeFormat { get; init; } = GetDefaultTimeFormat();

    /// <summary>
    ///     Gets the timezone used for displaying dates and times.
    ///     Defaults to <see cref="TimeZoneInfo.Local" />.
    /// </summary>
    public TimeZoneInfo TimeZone { get; init; } = TimeZoneInfo.Local;

    /// <summary>
    ///     Gets the combined format pattern for displaying date and time together,
    ///     constructed from <see cref="DateFormat" /> and <see cref="TimeFormat" />.
    /// </summary>
    public string TimestampFormat => $"{DateFormat} {TimeFormat}";

    private static string GetDefaultDateFormat()
    {
        var systemDatePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
        return DateFormats.Contains(systemDatePattern) ? systemDatePattern : DateFormats[0];
    }

    private static string GetDefaultTimeFormat()
    {
        var systemTimePattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
        return systemTimePattern.Contains("tt") ? TimeFormats[0] : TimeFormats[1];
    }
}
