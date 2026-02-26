using System.Text;
using Moq;
using Zylance.Contract.Models.File;
using Zylance.Core.Platform.Interfaces;
using Zylance.Core.Settings.Models;
using Zylance.Core.Settings.Services;
using Zylance.Core.Tests.TestUtils.Factories;
using Zylance.Core.Tests.TestUtils.Factories.Services;

namespace Zylance.Core.Tests;

public class SettingsServiceTests
{
    private readonly FileRef _fileRef;
    private readonly Mock<IFileProvider> _providerMock;
    private readonly SettingsService _service;

    public SettingsServiceTests()
    {
        _fileRef = new FileRef
        {
            Id = "mock",
            Filename = SettingsService.SettingsFilePath,
            ReadOnly = false,
        };
        _providerMock = FileServiceTestFactory.CreateMockProvider();
        _providerMock.Setup(p => p.GetAppDataFile(SettingsService.SettingsFilePath)).Returns(_fileRef);

        var fileService = FileServiceTestFactory.CreateFileService(_providerMock);
        _service = new SettingsService(fileService);
    }

    [Fact]
    public async Task LoadAppSettingsAsync_FileDoesNotExist_ReturnsDefault()
    {
        // Arrange
        FileServiceTestFactory.SetupExists(_providerMock, _fileRef, false);

        // Act
        var result = await _service.LoadAppSettingsAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(AppSettings.Default, result);
    }

    [Fact]
    public async Task LoadAppSettingsAsync_FileExists_DeserializesSettings()
    {
        // Arrange
        FileServiceTestFactory.SetupExists(_providerMock, _fileRef, true);
        // language=yaml
        var yaml = """
            dateTime:
              dateFormat: "yyyy-MM-dd"
              timeFormat: "HH:mm:ss"
            """;
        var yamlStream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));
        _providerMock.Setup(p => p.OpenFile(_fileRef)).Returns(yamlStream);

        // Act
        var result = await _service.LoadAppSettingsAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal("yyyy-MM-dd", result.DateTime.DateFormat);
        Assert.Equal("HH:mm:ss", result.DateTime.TimeFormat);
    }

    [Fact]
    public async Task SaveAppSettingsAsync_SerializesAndSavesYaml()
    {
        // Arrange
        FileServiceTestFactory.SetupExists(_providerMock, _fileRef, true);
        MemoryStream? savedStream = null;
        _providerMock
            .Setup(p => p.SaveFileAsync(_fileRef, It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Callback<FileRef, Stream, CancellationToken>(
                (_, stream, _) =>
                {
                    savedStream = new MemoryStream();
                    stream.CopyTo(savedStream);
                    savedStream.Position = 0;
                }
            )
            .Returns(Task.CompletedTask);
        var settings = new AppSettings
        {
            DateTime = new() { DateFormat = "yyyy-MM-dd", TimeFormat = "HH:mm:ss" },
        };

        // Act
        await _service.SaveAppSettingsAsync(settings, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(savedStream);
        var yaml = Encoding.UTF8.GetString(savedStream!.ToArray());
        Assert.Contains("dateFormat: yyyy-MM-dd", yaml);
    }

    [Fact]
    public async Task LoadAppSettingsAsync_RespectsCancellationToken()
    {
        // Arrange
        FileServiceTestFactory.SetupExists(_providerMock, _fileRef, true);
        _providerMock.Setup(p => p.GetAppDataFile(SettingsService.SettingsFilePath)).Returns(_fileRef);
        _providerMock.Setup(p => p.OpenFile(_fileRef)).Throws(new TaskCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _service.LoadAppSettingsAsync(TestContext.Current.CancellationToken)
        );
    }
}
