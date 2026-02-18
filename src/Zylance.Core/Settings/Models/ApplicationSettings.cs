namespace Zylance.Core.Settings.Models;

/// <summary>
///     Settings related to the user, such as preferences and configurations.
/// </summary>
public record UserSettings
{
    public string TimestampFormat { get; init; } = "yyyy-MM-dd HH:mm:ss";
}
