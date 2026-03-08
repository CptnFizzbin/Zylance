using System.Globalization;
using Zylance.Contract.Api.Settings;

namespace Zylance.Core.Settings.Models;

/// <summary>
///     Provides available date and time patterns used by the application and a
///     helper
///     to convert them into the transport DTO (<see cref="DateTimeOptionsData" />
///     ).
/// </summary>
public static class DateTimeOptions
{
    /// <summary>
    ///     Available date format patterns that users can choose from.
    /// </summary>
    /// <value>
    ///     A list of .NET date format strings. The special value "system" indicates
    ///     the current culture's short date pattern and will be replaced at runtime
    ///     with <see cref="SystemDatePattern" /> when displaying values.
    /// </value>
    public static readonly List<string> DatePatterns =
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
    /// <value>
    ///     A list of .NET time format strings. The special value "system" indicates
    ///     the current culture's short time pattern and will be replaced at runtime
    ///     with <see cref="SystemTimePattern" /> when displaying values.
    /// </value>
    public static readonly List<string> TimePatterns =
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

    private static readonly CultureInfo SystemCulture = CultureInfo.CurrentCulture;

    /// <summary>
    ///     The system (current culture) short date pattern.
    /// </summary>
    /// <remarks>
    ///     This value is obtained from <see cref="CultureInfo.CurrentCulture" />'s
    ///     <see cref="DateTimeFormatInfo.ShortDatePattern" /> and is useful when the
    ///     user selects the "system" option in <see cref="DatePatterns" />.
    /// </remarks>
    public static string SystemDatePattern => SystemCulture.DateTimeFormat.ShortDatePattern;

    /// <summary>
    ///     The system (current culture) short time pattern.
    /// </summary>
    /// <remarks>
    ///     This value is obtained from <see cref="CultureInfo.CurrentCulture" />'s
    ///     <see cref="DateTimeFormatInfo.ShortTimePattern" /> and is useful when the
    ///     user selects the "system" option in <see cref="TimePatterns" />.
    /// </remarks>
    public static string SystemTimePattern => SystemCulture.DateTimeFormat.ShortTimePattern;

    /// <summary>
    ///     Convert to a <see cref="DateTimeOptionsData" /> DTO used by the transport
    ///     layer.
    /// </summary>
    /// <returns>
    ///     A populated <see cref="DateTimeOptionsData" /> instance containing
    ///     available patterns and the system defaults.
    /// </returns>
    public static DateTimeOptionsData ToData()
    {
        var data = new DateTimeOptionsData();
        data.DatePatterns.AddRange(DatePatterns);
        data.TimePatterns.AddRange(TimePatterns);
        return data;
    }
}
