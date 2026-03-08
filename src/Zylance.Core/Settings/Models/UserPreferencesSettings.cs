using Zylance.Contract.Api.Settings;

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
    ///     Singleton default settings instance returned when no user preferences
    ///     file exists or when loading fails. This instance contains default
    ///     values and is safe to use as a fallback.
    /// </summary>
    public static readonly UserPreferencesSettings Default = new();

    /// <summary>
    ///     Date and time formatting and localization settings.
    /// </summary>
    public DateTimeSettings DateTime { get; init; } = new();

    /// <summary>
    ///     Convert this settings model into the transport/data contract used by the
    ///     settings persistence layer (<see cref="UserPreferencesData"/>).
    /// </summary>
    /// <returns>
    ///     A <see cref="UserPreferencesData"/> instance containing the serialized
    ///     representation of this settings object.
    /// </returns>
    public UserPreferencesData ToData()
    {
        return new() { DateTimeFormat = DateTime.ToData() };
    }

    /// <summary>
    ///     Create a <see cref="UserPreferencesSettings"/> instance from a
    ///     <see cref="UserPreferencesData"/> transport object.
    /// </summary>
    /// <param name="data">The data object deserialized from persistent storage.</param>
    /// <returns>
    ///     A new <see cref="UserPreferencesSettings"/> populated from <paramref name="data"/>.
    /// </returns>
    public static UserPreferencesSettings FromData(UserPreferencesData data)
    {
        return new() { DateTime = DateTimeSettings.FromData(data.DateTimeFormat) };
    }
}
