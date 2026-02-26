using System.Reflection;
using Zylance.Desktop.Providers;
using Zylance.Desktop.Services;
using Zylance.Desktop.Tests.TestUtils;
using Zylance.Desktop.Tests.TestUtils.Fixtures;

namespace Zylance.Desktop.Tests.Providers;

public class DesktopVaultProviderTests : IAsyncLifetime
{
    private string _appDataDir = null!;
    private HeadlessFileProvider _fileProvider = null!;
    private LocalFileService _fileService = null!;
    private string _tempDataDir = null!;
    private DesktopVaultProvider _vaultProvider = null!;

    public async ValueTask InitializeAsync()
    {
        _appDataDir = Path.Combine(Path.GetTempPath(), "Zylance.AppData", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_appDataDir);

        _tempDataDir = Path.Combine(Path.GetTempPath(), "Zylance.Temp", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDataDir);

        _fileProvider = new HeadlessFileProvider(_appDataDir, _tempDataDir);
        _fileService = new LocalFileService(_fileProvider);
        _vaultProvider = new DesktopVaultProvider(_fileService);
        await Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        _fileProvider.Dispose();

        try
        {
            if (Directory.Exists(_appDataDir))
                Directory.Delete(_appDataDir, true);

            if (Directory.Exists(_tempDataDir))
                Directory.Delete(_tempDataDir, true);
        }
        catch
        {
            // Ignore cleanup errors
        }

        await Task.CompletedTask;
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task GetRecentVaults_ReturnsVaultsFromSettings()
    {
        // Arrange
        var vault1Path = Path.Combine(_tempDataDir, "vault1.zlv");
        var vault2Path = Path.Combine(_tempDataDir, "vault2.zlv");

        var method = typeof(DesktopVaultProvider).GetMethod(
            "RecordRecentVault",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        // Record vaults in specific order
        var task1 = (Task)method!.Invoke(_vaultProvider, [vault1Path])!;
        await task1;

        var task2 = (Task)method.Invoke(_vaultProvider, [vault2Path])!;
        await task2;

        // Act
        var recentVaults = await _vaultProvider.GetRecentVaults();

        // Assert
        Assert.Equal(2, recentVaults.Count);
        // Vault names are extracted from paths (without extension)
        Assert.Equal("vault2", recentVaults[0].Name);
        Assert.Equal("vault1", recentVaults[1].Name);
    }

    [Fact]
    public async Task OpenVault_UpdatesRecentVaultsList()
    {
        // Arrange - Create a real vault file
        var vaultPath = Path.Combine(_tempDataDir, "real-vault.zlv");
        var emptyVaultFixture = FixtureUtils.GetFixturePath("Vaults/EmptyVault.zlv");
        File.Copy(emptyVaultFixture, vaultPath);

        // Configure file provider to return the vault path when SelectFileAsync is called
        _fileProvider.OnSelectFile(() => vaultPath);

        // Act - Call OpenVault which should update recent vaults
        var vault = await _vaultProvider.OpenVault();

        // Assert
        Assert.NotNull(vault);
        var recentVaults = await _vaultProvider.GetRecentVaults();
        Assert.NotEmpty(recentVaults);
        Assert.Equal("real-vault", recentVaults[0].Name);
    }

    [Fact]
    public async Task CreateVault_UpdatesRecentVaultsList()
    {
        // Arrange
        var newVaultPath = Path.Combine(_tempDataDir, "new-vault.zlv");

        // Configure file provider to return the new vault path when CreateFileAsync is called
        _fileProvider.OnCreateFile(() => newVaultPath);

        // Act - Call CreateVault which should update recent vaults
        var vault = await _vaultProvider.CreateVault();

        // Assert
        Assert.NotNull(vault);
        var recentVaults = await _vaultProvider.GetRecentVaults();
        Assert.NotEmpty(recentVaults);
        Assert.Equal("new-vault", recentVaults[0].Name);
    }

    [Fact]
    public async Task OpenVault_AddsToRecentVaultsList()
    {
        // Arrange - Create fixture vault
        var openVaultPath = Path.Combine(_tempDataDir, "open-vault.zlv");
        var emptyVaultFixture = FixtureUtils.GetFixturePath("Vaults/EmptyVault.zlv");
        File.Copy(emptyVaultFixture, openVaultPath);

        // Configure file provider to return the vault path
        _fileProvider.OnSelectFile(() => openVaultPath);

        // Act - Open vault
        await _vaultProvider.OpenVault();

        // Assert - Vault should be added to recent vaults
        var recentVaults = await _vaultProvider.GetRecentVaults();
        Assert.Single(recentVaults);
        Assert.Equal("open-vault", recentVaults[0].Name);
    }

    [Fact]
    public async Task CreateVault_AddsToRecentVaultsList()
    {
        // Arrange - Create and add an existing vault to recent vaults
        var openVaultPath = Path.Combine(_tempDataDir, "open-vault.zlv");
        var emptyVaultFixture = FixtureUtils.GetFixturePath("Vaults/EmptyVault.zlv");
        File.Copy(emptyVaultFixture, openVaultPath);

        _fileProvider.OnSelectFile(() => openVaultPath);
        await _vaultProvider.OpenVault();

        var createVaultPath = Path.Combine(_tempDataDir, "create-vault.zlv");
        _fileProvider.OnCreateFile(() => createVaultPath);

        // Act - Create a new vault
        await _vaultProvider.CreateVault();

        // Assert - Created vault should be at front, with opened vault second
        var recentVaults = await _vaultProvider.GetRecentVaults();
        Assert.Equal(2, recentVaults.Count);
        Assert.Equal("create-vault", recentVaults[0].Name);
        Assert.Equal("open-vault", recentVaults[1].Name);
    }
}
