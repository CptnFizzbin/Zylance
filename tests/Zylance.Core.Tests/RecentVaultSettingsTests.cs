using System.Text;
using Moq;
using Zylance.Contract.Models.File;
using Zylance.Core.Settings.Models;
using Zylance.Core.System.Services;
using Zylance.Core.Tests.TestUtils.Factories;

namespace Zylance.Core.Tests;

public class RecentVaultSettingsTests
{
    private readonly FileService _fileService;

    private string? _recentVaultsYaml;

    public RecentVaultSettingsTests()
    {
        var recentVaultPath = RecentVaultSettings.SettingsFilePath;

        var providerMock = FileServiceTestFactory.CreateMockProvider();
        var fileRef = new FileRef { Id = "test", Filename = recentVaultPath };

        providerMock.Setup(p => p.Exists(fileRef)).Returns(() => _recentVaultsYaml is not null);

        providerMock.Setup(p => p.GetAppDataFile(recentVaultPath)).Returns(fileRef);

        providerMock
            .Setup(p => p.OpenFile(fileRef))
            .Returns(() =>
            {
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(_recentVaultsYaml ?? ""));
                stream.Position = 0;
                return stream;
            });

        providerMock
            .Setup(p => p.SaveFileAsync(fileRef, It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Callback<FileRef, Stream, CancellationToken>(
                (_, stream, _) =>
                {
                    stream.Position = 0;
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    _recentVaultsYaml = reader.ReadToEnd();
                }
            )
            .Returns(Task.CompletedTask);

        _fileService = new FileService(providerMock.Object);
    }

    [Fact]
    public async Task WriteAsync_CanWriteToDisk()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var vaultPath = "/usr/home/jimmy/zylance1.zlv";

        // Act
        await RecentVaultSettings.WriteAsync(_fileService, "local", [vaultPath], cancellationToken);

        var savedVaults = await RecentVaultSettings.ReadAsync(_fileService, "local", cancellationToken);

        // Assert
        Assert.Equal(vaultPath, savedVaults[0]);
    }

    [Fact]
    public async Task ReadAsync_WhenNoValueIsSaved_ReturnsDefault()
    {
        // Act
        var recentVaults = await RecentVaultSettings.ReadAsync(
            _fileService,
            "unknown",
            TestContext.Current.CancellationToken
        );

        // Assert
        Assert.Empty(recentVaults);
    }

    [Fact]
    public async Task WriteAsync_DifferentProviders_DoesNotOverwriteOtherProviderData()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        var localVaultPath = "/usr/home/jimmy/zylance.zlv";
        var remoteVaultUrl = "https://example.local/zylance";

        // Act
        await RecentVaultSettings.WriteAsync(_fileService, "local", [localVaultPath], cancellationToken);
        await RecentVaultSettings.WriteAsync(_fileService, "remote", [remoteVaultUrl], cancellationToken);

        var savedLocalVaults = await RecentVaultSettings.ReadAsync(_fileService, "local", cancellationToken);
        var savedRemoteVaults = await RecentVaultSettings.ReadAsync(_fileService, "remote", cancellationToken);

        // Assert
        Assert.Equal(localVaultPath, savedLocalVaults[0]);
        Assert.Equal(remoteVaultUrl, savedRemoteVaults[0]);
    }
}
