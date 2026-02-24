using Zylance.Core.Settings.Models;

namespace Zylance.Core.Settings.Services;

/// <summary>
///     Manages application settings including date/time formatting preferences.
/// </summary>
public class SettingsService
{
    /// <summary>
    ///     Gets the current date and time display settings including format patterns and timezone.
    /// </summary>
    public DateAndTimeSettings DateAndTimeSettings { get; set; } = new();

    /// <summary>
    ///     Persists the current settings to storage.
    /// </summary>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public Task SaveAsync()
    {
        // TODO: save vault settings to vault
        // TODO: save application settings to file
        throw new NotImplementedException();
    }
}
