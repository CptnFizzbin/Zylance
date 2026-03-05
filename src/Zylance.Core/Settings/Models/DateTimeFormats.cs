namespace Zylance.Core.Settings.Models;

public static class DateTimeFormats
{
    /// <summary>
    ///     Available date format patterns that users can choose from.
    /// </summary>
    public static readonly List<string> AvailableDateFormats =
    [
        "system",
        // 2026-05-24
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
    /// </summary>
    public static readonly List<string> AvailableTimeFormats =
    [
        "system",
        // 10:30 PM
        "h:mm tt",
        // "22:30",
        "HH:mm",
        // "10:30:45 PM",
        "h:mm:ss tt",
        // "22:30:45",
        "HH:mm:ss",
    ];
}
