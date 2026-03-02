using Zylance.Contract.Models.File;
using Zylance.Core.Platform;
using Zylance.Core.Platform.Interfaces;

namespace Zylance.Core.Tests.TestUtils.Mocks;

/// <summary>
///     A test implementation of <see cref="ILocalFileProvider" /> that uses an
///     isolated temp directory per test run. Provides helpers for creating files,
///     copying fixtures, and queueing the next file returned by
///     <see cref="SelectFileAsync" /> / <see cref="CreateFileAsync" />.
/// </summary>
/// <remarks>
///     Directory layout:
///     <c>{system.tmp}/Zylance/tests/{guid}/app/</c>  - app data files
///     <c>{system.tmp}/Zylance/tests/{guid}/temp/</c> - temp files
/// </remarks>
public class TestFileProvider : ILocalFileProvider, IDisposable
{
    private readonly string _appDataPath;

    private readonly Dictionary<string, string> _fileReferences = [];
    private readonly Lock _lock = new();
    private readonly string _tempDataPath;
    private string? _nextCreateFilePath;

    private string? _nextSelectFilePath;

    public TestFileProvider()
    {
        var sessionId = Guid.NewGuid().ToString();
        RootPath = Path.Combine(Path.GetTempPath(), "Zylance", "tests", sessionId);
        _appDataPath = Path.Combine(RootPath, "app");
        _tempDataPath = Path.Combine(RootPath, "temp");

        Directory.CreateDirectory(_appDataPath);
        Directory.CreateDirectory(_tempDataPath);
    }

    /// <summary>
    ///     The root directory for this test session. Useful for constructing paths
    ///     to pass back into <see cref="QueueSelectFile" /> /
    ///     <see cref="QueueCreateFile" />.
    /// </summary>
    public string RootPath { get; }

    /// <summary>
    ///     Deletes the entire test session directory and all its contents.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(RootPath))
            Directory.Delete(RootPath, true);
    }

    /// <inheritdoc />
    public bool Exists(FileRef fileRef)
    {
        var path = GetFilePath(fileRef);
        return File.Exists(path);
    }

    /// <inheritdoc />
    public Task<FileRef> SelectFileAsync(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = true
    )
    {
        var path =
            _nextSelectFilePath
            ?? throw new InvalidOperationException(
                "Unexpected call to SelectFileAsync - no file queued. Call QueueSelectFile() in your test setup."
            );

        _nextSelectFilePath = null;
        return Task.FromResult(RegisterFileReference(path, readOnly));
    }

    /// <inheritdoc />
    public Task<FileRef> CreateFileAsync(
        string? title = null,
        string? defaultPath = null,
        (string Name, string[] Extensions)[]? filters = null
    )
    {
        var path =
            _nextCreateFilePath
            ?? throw new InvalidOperationException(
                "Unexpected call to CreateFileAsync - no file queued. Call QueueCreateFile() in your test setup."
            );

        _nextCreateFilePath = null;
        return Task.FromResult(RegisterFileReference(path, FileRefFlags.ReadWrite));
    }

    /// <inheritdoc />
    public Stream OpenFile(FileRef fileRef)
    {
        var filePath = GetFilePath(fileRef);
        return File.Exists(filePath)
            ? File.Open(filePath, FileMode.Open)
            : throw new FileNotFoundException($"File not found: {filePath}", filePath);
    }

    /// <inheritdoc />
    public FileRef GetTempFile(string path)
    {
        var fullPath = Path.Combine(_tempDataPath, path);
        EnsureDirectory(fullPath);
        return RegisterFileReference(fullPath, FileRefFlags.ReadWrite);
    }

    /// <inheritdoc />
    public FileRef GetAppDataFile(string path)
    {
        var fullPath = Path.Combine(_appDataPath, path);
        EnsureDirectory(fullPath);
        return RegisterFileReference(fullPath, FileRefFlags.ReadWrite);
    }

    /// <inheritdoc />
    public string GetFilePath(FileRef fileRef)
    {
        lock (_lock)
        {
            if (_fileReferences.TryGetValue(fileRef.Id, out var filePath))
                return filePath;
        }

        throw new ArgumentException($"Invalid FileRef ID: {fileRef.Id}", nameof(fileRef));
    }

    /// <inheritdoc />
    public Task TouchFileAsync(FileRef fileRef)
    {
        return Exists(fileRef) ? Task.CompletedTask : SaveFileAsync(fileRef, new MemoryStream());
    }

    /// <inheritdoc />
    public Task SaveFileAsync(FileRef fileRef, Stream content)
    {
        if (fileRef.ReadOnly)
            throw new UnauthorizedAccessException($"Cannot save to read-only file reference: {fileRef.Id}");

        var filePath = GetFilePath(fileRef);
        EnsureDirectory(filePath);

        using var fileStream = File.Create(filePath);
        return content.CopyToAsync(fileStream);
    }

    /// <inheritdoc />
    public Task DeleteFileAsync(FileRef fileRef)
    {
        if (fileRef.ReadOnly)
            throw new UnauthorizedAccessException($"Cannot delete read-only file reference: {fileRef.Id}");

        var filePath = GetFilePath(fileRef);

        lock (_lock)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            _fileReferences.Remove(fileRef.Id);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Creates an empty file at the given path under the test root and returns
    ///     a registered <see cref="FileRef" /> for it.
    /// </summary>
    /// <param name="relativePath">Relative path under the test root directory.</param>
    /// <param name="readOnly">Whether the file ref should be read-only.</param>
    public FileRef CreateFile(string relativePath, bool readOnly = FileRefFlags.ReadWrite)
    {
        var fullPath = Path.Combine(RootPath, relativePath);
        EnsureDirectory(fullPath);
        File.WriteAllBytes(fullPath, []);
        return RegisterFileReference(fullPath, readOnly);
    }

    /// <summary>
    ///     Copies a fixture file into the test directory and returns a registered
    ///     <see cref="FileRef" /> for it.
    /// </summary>
    /// <param name="fixtureAbsolutePath">Absolute path to the source fixture file.</param>
    /// <param name="destinationRelativePath">Relative destination path under the test root.</param>
    /// <param name="readOnly">Whether the file ref should be read-only.</param>
    public FileRef CopyFixture(
        string fixtureAbsolutePath,
        string destinationRelativePath,
        bool readOnly = FileRefFlags.ReadWrite
    )
    {
        var destinationPath = Path.Combine(RootPath, destinationRelativePath);
        EnsureDirectory(destinationPath);
        File.Copy(fixtureAbsolutePath, destinationPath, true);
        return RegisterFileReference(destinationPath, readOnly);
    }

    /// <summary>
    ///     Sets the file path that will be returned on the next call to
    ///     <see cref="SelectFileAsync" />. Only consumed once.
    /// </summary>
    /// <param name="absolutePath">Absolute path to the file to select.</param>
    public void QueueSelectFile(string absolutePath)
    {
        _nextSelectFilePath = absolutePath;
    }

    /// <summary>
    ///     Sets the file path that will be returned on the next call to
    ///     <see cref="CreateFileAsync" />. Only consumed once.
    /// </summary>
    /// <param name="absolutePath">Absolute path to the file to create.</param>
    public void QueueCreateFile(string absolutePath)
    {
        _nextCreateFilePath = absolutePath;
    }

    private FileRef RegisterFileReference(string filePath, bool readOnly)
    {
        var fileRef = new FileRef
        {
            Id = Guid.NewGuid().ToString(),
            Filename = Path.GetFileName(filePath),
            ReadOnly = readOnly,
        };

        lock (_lock)
        {
            _fileReferences[fileRef.Id] = filePath;
        }

        return fileRef;
    }

    private static void EnsureDirectory(string filePath)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);
    }
}
