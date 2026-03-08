using System.Text;
using Serilog;
using Zylance.Core.Logging;
using Zylance.Core.Settings.Models;
using Zylance.Core.System.Services;

namespace Zylance.Core.Settings.Services;

/// <summary>
///     Service responsible for loading and saving user preferences.
///     Filesystem access is delegated to <see cref="FileService" />.
/// </summary>
public class UserPreferencesService
{
    private readonly FileService _fileService;
    private readonly ILogger _log = ZyLogger.ForContext<UserPreferencesService>();

    public UserPreferencesService(FileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    ///     Loads the <see cref="UserPreferencesSettings" /> from the app data folder
    ///     via <see cref="FileService" />.
    ///     If the file does not exist or deserialization fails, returns
    ///     <see cref="UserPreferencesSettings.Default" />.
    /// </summary>
    public async Task<UserPreferencesSettings> LoadUserPreferencesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var fileRef = _fileService.GetAppDataFile(UserPreferencesSettings.FilePath);

            if (!_fileService.Exists(fileRef))
                return UserPreferencesSettings.Default;

            var yaml = await _fileService.WithFileAsync(
                fileRef,
                async stream =>
                {
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    return await reader.ReadToEndAsync(cancellationToken);
                }
            );

            return SettingsUtils.YamlDeserializer.Deserialize<UserPreferencesSettings>(yaml);
        }
        catch (Exception ex)
        {
            _log.Warning(ex, "Failed to load user preferences, returning defaults");
            return UserPreferencesSettings.Default;
        }
    }

    /// <summary>
    ///     Saves the provided <see cref="UserPreferencesSettings" /> to the app data
    ///     folder using <see cref="FileService" />.
    /// </summary>
    public async Task<UserPreferencesSettings> SaveUserPreferencesAsync(
        UserPreferencesSettings settings,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var yaml = SettingsUtils.YamlSerializer.Serialize(settings);
            var bytes = Encoding.UTF8.GetBytes(yaml);

            await using var ms = new MemoryStream(bytes);

            var fileRef = _fileService.GetAppDataFile(UserPreferencesSettings.FilePath);
            await _fileService.SaveFileAsync(fileRef, ms, cancellationToken);

            return await LoadUserPreferencesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _log.Error(ex, "Failed to save user preferences");
            throw;
        }
    }
}
