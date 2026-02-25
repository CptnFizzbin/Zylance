using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Zylance.Contract.Models.File;
using Zylance.Core.Settings.Models;
using Zylance.Core.System.Services;

namespace Zylance.Core.Settings.Services;

/// <summary>
///     Manages application settings including date/time formatting preferences.
/// </summary>
public class SettingsService(FileService fileService)
{
    private readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private FileRef GetSettingsFileRef()
    {
        return fileService.GetAppDataFile("settings.yaml");
    }

    /// <summary>
    ///     Loads application settings from the settings.yaml file in app data.
    ///     Returns default settings if the file does not exist.
    /// </summary>
    /// <param name="token">Cancellation token for async file operations.</param>
    /// <returns>Deserialized SettingsModel instance.</returns>
    public async Task<SettingsModel> LoadAsync(CancellationToken token = default)
    {
        var settingsFileRef = GetSettingsFileRef();

        if (!fileService.Exists(settingsFileRef))
            return SettingsModel.Default;

        var yaml = await fileService.ReadFileAsync(settingsFileRef, token);
        var settingsModel = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build()
            .Deserialize<SettingsModel>(yaml);

        return settingsModel;
    }

    /// <summary>
    ///     Persists the current settings to storage.
    /// </summary>
    /// <returns>A task representing the asynchronous save operation.</returns>
    public async Task SaveAsync(SettingsModel settingsModel, CancellationToken token = default)
    {
        var yaml = _serializer.Serialize(settingsModel);
        var settingsFileRef = GetSettingsFileRef();
        await fileService.SaveFileAsync(settingsFileRef, yaml, token);

        // TODO: save vault settings to vault
    }
}
