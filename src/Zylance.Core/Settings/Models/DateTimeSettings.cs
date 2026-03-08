using Zylance.Contract.Api.Settings;

namespace Zylance.Core.Settings.Models;

/// <summary>
///     Settings for date and time display formatting, including format patterns
///     and timezone preferences.
/// </summary>
public record DateTimeSettings
{
    /// <summary>
    ///     The format pattern used for displaying dates, or "system"
    /// </summary>
    public string DatePattern { get; init; } = "system";

    /// <summary>
    ///     The format pattern used for displaying times, or "system"
    /// </summary>
    public string TimePattern { get; init; } = "system";

    /// <summary>
    ///     Convert this model to the transport/data contract
    ///     <see cref="DateTimeFormatData" />.
    /// </summary>
    /// <returns>
    ///     A new <see cref="DateTimeFormatData" /> instance containing the values
    ///     from this <see cref="DateTimeSettings" />.
    /// </returns>
    public DateTimeFormatData ToData()
    {
        return new DateTimeFormatData { DatePattern = DatePattern, TimePattern = TimePattern };
    }

    /// <summary>
    ///     Create a <see cref="DateTimeSettings" /> from the transport/data contract
    ///     <see cref="DateTimeFormatData" />.
    /// </summary>
    /// <param name="dateTimeFormatData">
    ///     The data contract instance to convert from
    /// </param>
    /// <returns>
    ///     A new <see cref="DateTimeSettings" /> populated from
    ///     <paramref name="dateTimeFormatData" />.
    /// </returns>
    public static DateTimeSettings FromData(DateTimeFormatData dateTimeFormatData)
    {
        return new DateTimeSettings
        {
            DatePattern = dateTimeFormatData.DatePattern,
            TimePattern = dateTimeFormatData.TimePattern,
        };
    }
}
