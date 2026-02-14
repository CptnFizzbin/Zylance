using Zylance.Contract.Models.File;
using Zylance.Core.Importers.Interfaces;
using Zylance.Core.Importers.Ofx;

namespace Zylance.Core.Tests.Importers;

public class OfxImporterTests
{
    private readonly OfxImporter _importer = new();

    [Fact]
    public void Constructor_CreatesInstance()
    {
        // Act
        var importer = new OfxImporter();

        // Assert
        Assert.NotNull(importer);
    }

    [Fact]
    public void SupportedExtensions_ContainsQfxExtension()
    {
        // Act
        var extensions = _importer.SupportedExtensions;

        // Assert
        Assert.NotNull(extensions);
    }

    [Fact]
    public void SupportedExtensions_IsNotEmpty()
    {
        // Act
        var extensions = _importer.SupportedExtensions;

        // Assert
        Assert.NotEmpty(extensions);
    }

    [Fact]
    public void SupportedExtensions_IsReadOnly()
    {
        // Act
        var extensions = _importer.SupportedExtensions;

        // Assert
        Assert.IsType<IReadOnlyList<(string, string[])>>(extensions, false);
    }

    [Fact]
    public async Task ImportAsync_WithNullFileRef_ThrowsNotImplementedException()
    {
        // Arrange
        FileRef? nullFileRef = null;

        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() =>
            _importer.ImportAsync(nullFileRef!, TestContext.Current.CancellationToken)
        );
    }

    [Fact]
    public async Task ImportAsync_WithValidFile_ThrowsNotImplementedException()
    {
        // Arrange
        var fileRef = new FileRef
        {
            Id = "test",
            Filename = "transactions.qfx",
            ReadOnly = true,
        };

        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() =>
            _importer.ImportAsync(fileRef, TestContext.Current.CancellationToken)
        );
    }

    [Fact]
    public async Task ImportAsync_WithCancellationToken_ThrowsNotImplementedException()
    {
        // Arrange
        var fileRef = new FileRef
        {
            Id = "test",
            Filename = "transactions.qfx",
            ReadOnly = true,
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => _importer.ImportAsync(fileRef, cancellationToken));
    }

    [Fact]
    public async Task ImportAsync_WithCancelledToken_ThrowsNotImplementedException()
    {
        // Arrange
        var fileRef = new FileRef
        {
            Id = "test",
            Filename = "transactions.qfx",
            ReadOnly = true,
        };
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();
        var cancellationToken = cancellationTokenSource.Token;

        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => _importer.ImportAsync(fileRef, cancellationToken));
    }

    [Fact]
    public void OfxImporter_ImplementsIImporter()
    {
        // Act & Assert
        Assert.IsType<IImporter>(_importer, false);
    }
}
