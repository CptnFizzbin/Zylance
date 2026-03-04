using Zylance.Contract.Models.Vault;
using Zylance.Core.Settings.Services;
using Zylance.Core.System.Services;
using Zylance.Core.Vault.Interfaces;
using Zylance.Core.Vault.Models;

namespace Zylance.Core.Vault.Services;

public class RecentVaultsService(FileService fileService)
{
    /// <summary>
    ///     The relative path to the recent vaults settings file within app data.
    ///     This YAML file stores recent vault information for each provider.
    /// </summary>
    public const string SettingsFilePath = "user-settings/recent-vaults.yaml";

    public async Task AddVaultAsync(IVault vault)
    {
        var recentVaults = await ReadSettingsAsync();
        recentVaults.AddVault(vault);
        await WriteSettingsAsync(recentVaults);
    }

    public async Task<List<RecentVaultRef>> GetRecentVaultsAsync(string providerId)
    {
        var recentVaults = await ReadSettingsAsync();
        return recentVaults.GetValueOrDefault(providerId) ?? [];
    }

    private async Task<RecentVaultsList> ReadSettingsAsync(CancellationToken cancellationToken = default)
    {
        var fileRef = fileService.GetAppDataFile(SettingsFilePath);
        if (!fileService.Exists(fileRef))
            return [];

        var yaml = await fileService.ReadFileAsync(fileRef, cancellationToken);
        return SettingsUtils.YamlDeserializer.Deserialize<RecentVaultsList>(yaml);
    }

    private async Task WriteSettingsAsync(RecentVaultsList recentVaults, CancellationToken cancellationToken = default)
    {
        var fileRef = fileService.GetAppDataFile(SettingsFilePath);
        var yaml = SettingsUtils.YamlSerializer.Serialize(recentVaults);
        await fileService.SaveFileAsync(fileRef, yaml, cancellationToken);
    }
}
