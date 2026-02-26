namespace Zylance.Core.Settings.Models;

/// <summary>
///     Represents the application settings model, containing user preferences and
///     configuration options.
/// </summary>
public record AppSettings
{
    /// <summary>
    ///     Default settings model instance with default values for all properties.
    /// </summary>
    public static readonly AppSettings Default = new();

    /// <summary>
    ///     Date and time formatting and localization settings for the application.
    /// </summary>
    public DateAndTimeSettings DateTime { get; init; } = new();
}
