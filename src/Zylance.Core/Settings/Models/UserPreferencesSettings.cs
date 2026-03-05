namespace Zylance.Core.Settings.Models;

/// <summary>
///     Model representing persisted user preferences for the application.
///     This model owns the FilePath where preferences are stored (relative to app
///     data).
/// </summary>
public record UserPreferencesSettings
{
    /// <summary>
    ///     Relative path within the application's data folder where preferences are
    ///     stored.
    /// </summary>
    public const string FilePath = "config/user-preferences.yaml";

    /// <summary>
    ///     Default settings instance.
    /// </summary>
    public static readonly UserPreferencesSettings Default = new();

    /// <summary>
    ///     Date and time formatting and localization settings.
    /// </summary>
    public DateAndTimeSettings DateTime { get; init; } = new();
}
