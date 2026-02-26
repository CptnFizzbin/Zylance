using Zylance.Core.Settings.Services;
using Zylance.Core.System.Services;

namespace Zylance.Core.Settings.Models;

/// <summary>
///     Contains recent vault information for different providers, stored in a YAML
///     file in app data. Each provider can
///     store its own data structure under its name.
/// </summary>
public class RecentVaultSettings
{
    /// <summary>
    ///     The relative path to the recent vaults settings file within app data.
    ///     This YAML file stores recent vault information for each provider.
    /// </summary>
    public const string SettingsFilePath = "user-settings/recent-vaults.yaml";

    /// <summary>
    ///     Dictionary storing provider-specific recent vault information, keyed by
    ///     provider name.
    /// </summary>
    public Dictionary<string, List<string>> Providers { get; set; } = [];

    /// <summary>
    ///     Reads the recent vault settings for a specific provider from the YAML file.
    /// </summary>
    /// <param name="fileService">The file service used to access the settings file.</param>
    /// <param name="providerName">
    ///     The name of the provider whose recent vault settings
    ///     to read.
    /// </param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    ///     The recent vault settings for the specified provider, or an empty list if not found.
    /// </returns>
    public static async Task<List<string>> ReadAsync(
        FileService fileService,
        string providerName,
        CancellationToken cancellationToken = default
    )
    {
        var settings = await ReadSettingsAsync(fileService, cancellationToken);
        return settings.Providers.GetValueOrDefault(providerName) ?? [];
    }

    /// <summary>
    ///     Writes the recent vault settings for a specific provider to the YAML file.
    /// </summary>
    /// <param name="fileService">The file service used to access the settings file.</param>
    /// <param name="providerName">
    ///     The name of the provider whose recent vault settings
    ///     to write.
    /// </param>
    /// <param name="data">The provider data to write for the specified provider.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public static async Task WriteAsync(
        FileService fileService,
        string providerName,
        List<string> data,
        CancellationToken cancellationToken = default
    )
    {
        var settings = await ReadSettingsAsync(fileService, cancellationToken);
        settings.Providers[providerName] = data;
        await WriteSettingsAsync(fileService, settings, cancellationToken);
    }

    private static async Task<RecentVaultSettings> ReadSettingsAsync(
        FileService fileService,
        CancellationToken cancellationToken = default
    )
    {
        var fileRef = fileService.GetAppDataFile(SettingsFilePath);
        if (!fileService.Exists(fileRef))
            return new();

        var yaml = await fileService.ReadFileAsync(fileRef, cancellationToken);
        return SettingsService.YamlDeserializer.Deserialize<RecentVaultSettings>(yaml);
    }

    private static async Task WriteSettingsAsync(
        FileService fileService,
        RecentVaultSettings settings,
        CancellationToken cancellationToken = default
    )
    {
        var fileRef = fileService.GetAppDataFile(SettingsFilePath);
        var yaml = SettingsService.YamlSerializer.Serialize(settings);
        await fileService.SaveFileAsync(fileRef, yaml, cancellationToken);
    }
}
