using Serilog;
using Zylance.Contract.Models.File;
using Zylance.Core.Logging;
using Zylance.Core.Platform;
using Zylance.Core.Platform.Interfaces;

namespace Zylance.Desktop.Providers;

/// <summary>
///     Base class that provides file management utilities and stores file
///     references for the desktop application.
/// </summary>
/// <remarks>
///     Constructor for LocalFileProvider.
/// </remarks>
/// <param name="appDataPath">Path to app data directory.</param>
/// <param name="tempDataPath">Path to temp directory.</param>
public abstract class LocalFileProvider(string appDataPath, string tempDataPath) : ILocalFileProvider, IDisposable
{
    private static readonly ILogger Log = ZyLogger.ForContext<LocalFileProvider>();

    // Store file references in memory - maps FileRef IDs to actual file paths
    private readonly Dictionary<string, string> _fileReferences = [];
    private readonly Lock _lock = new();

    private bool _disposed;

    /// <summary>
    ///     Cleans up the session temp directory and all its contents.
    /// </summary>
    public void Dispose()
    {
        Log.Information("Disposing LocalFileProvider and cleaning up temp directory at {TempDataPath}", tempDataPath);
        if (_disposed)
            return;

        try
        {
            // Clean up the session temp directory if it exists
            if (Directory.Exists(tempDataPath))
                Directory.Delete(tempDataPath, true);
        }
        catch (Exception ex)
        {
            // Log but don't throw - cleanup is best-effort
            Console.Error.WriteLine($"Warning: Failed to clean up temp directory {tempDataPath}: {ex.Message}");
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Checks whether the backing file for a fileRef exists.
    /// </summary>
    /// <param name="fileRef">ref to check.</param>
    public bool Exists(FileRef fileRef)
    {
        var path = GetFilePath(fileRef);
        return File.Exists(path);
    }

    /// <summary>
    ///     Prompts the user to select an existing file and returns a FileRef
    ///     representing it.
    /// </summary>
    /// <param name="title">Optional title for the file dialog.</param>
    /// <param name="filters">Optional file type filters.</param>
    /// <param name="readOnly">Whether the file should be opened as read-only.</param>
    /// <param name="token">Cancellation token.</param>
    public abstract Task<FileRef> SelectFileAsync(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = FileRefFlags.ReadOnly,
        CancellationToken token = default
    );

    /// <summary>
    ///     Prompts the user to create a file and returns a FileRef for the created
    ///     file.
    /// </summary>
    /// <param name="title">Optional title for the file dialog.</param>
    /// <param name="defaultPath">Optional default path for the new file.</param>
    /// <param name="filters">Optional file type filters.</param>
    /// <param name="token">Cancellation token.</param>
    public abstract Task<FileRef> CreateFileAsync(
        string? title = null,
        string? defaultPath = null,
        (string Name, string[] Extensions)[]? filters = null,
        CancellationToken token = default
    );

    /// <summary>
    ///     Saves the provided stream content to the file referenced by the FileRef.
    /// </summary>
    /// <param name="fileRef">Reference to the file to save.</param>
    /// <param name="content">Content stream to write to disk.</param>
    /// <param name="token">Cancellation token.</param>
    public Task SaveFileAsync(FileRef fileRef, Stream content, CancellationToken token = default)
    {
        if (fileRef.ReadOnly)
            throw new UnauthorizedAccessException($"Cannot save to read-only file reference: {fileRef.Id}");

        var filePath = GetFilePath(fileRef);
        if (File.Exists(filePath))
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.IsReadOnly)
                throw new UnauthorizedAccessException($"Cannot save to read-only file: {filePath}");
        }

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        using var fileStream = File.Create(filePath);

        return content.CopyToAsync(fileStream, token);
    }

    /// <summary>
    ///     Deletes the file referenced by the given FileRef and removes its internal
    ///     mapping.
    /// </summary>
    /// <param name="fileRef">Reference to the file to delete.</param>
    /// <param name="token">Cancellation token.</param>
    public Task DeleteFileAsync(FileRef fileRef, CancellationToken token = default)
    {
        if (fileRef.ReadOnly)
            throw new UnauthorizedAccessException($"Cannot delete read-only file reference: {fileRef.Id}");

        var filePath = GetFilePath(fileRef);

        lock (_lock)
        {
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.IsReadOnly)
                    throw new UnauthorizedAccessException($"Cannot delete read-only file: {filePath}");

                File.Delete(filePath);
            }

            _fileReferences.Remove(fileRef.Id);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Returns a FileRef for a path located in the session-specific temp
    ///     directory.
    /// </summary>
    /// <param name="path">Relative path under the temp directory.</param>
    public FileRef GetTempFile(string path)
    {
        var tempPath = Path.Combine(tempDataPath, path);
        var directory = Path.GetDirectoryName(tempPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return CreateFileReference(tempPath, FileRefFlags.ReadWrite);
    }

    /// <summary>
    ///     Retrieves the actual file path for a FileRef.
    /// </summary>
    /// <param name="fileRef">Reference to the file.</param>
    public string GetFilePath(FileRef fileRef)
    {
        lock (_lock)
        {
            if (_fileReferences.TryGetValue(fileRef.Id, out var filePath))
                return filePath;
        }

        throw new ArgumentException($"Invalid FileRef ID: {fileRef.Id}", nameof(fileRef));
    }

    /// <summary>
    ///     Opens a stream for the file referenced by the provided FileRef.
    /// </summary>
    /// <param name="fileRef">Reference to the file to open.</param>
    public Stream OpenFile(FileRef fileRef)
    {
        var filePath = GetFilePath(fileRef);
        var fileAccess = fileRef.ReadOnly ? FileAccess.Read : FileAccess.ReadWrite;

        return File.Exists(filePath)
            ? File.Open(filePath, FileMode.Open, fileAccess)
            : throw new FileNotFoundException($"File not found: {filePath}", filePath);
    }

    /// <summary>
    ///     Returns a FileRef for a path located in the application's AppData
    ///     directory.
    /// </summary>
    /// <param name="path">Relative path under the app data directory.</param>
    public FileRef GetAppDataFile(string path)
    {
        var fullPath = Path.Combine(appDataPath, path);
        var directory = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return CreateFileReference(fullPath, FileRefFlags.ReadWrite);
    }

    /// <summary>
    ///     Ensures the file exists by saving an empty content string to it.
    /// </summary>
    /// <param name="fileRef">Reference to the file to touch.</param>
    /// <param name="token">Cancellation token.</param>
    public Task TouchFileAsync(FileRef fileRef, CancellationToken token = default)
    {
        return Exists(fileRef) ? Task.CompletedTask : SaveFileAsync(fileRef, new MemoryStream(), token);
    }

    /// <summary>
    ///     Creates a FileRef from a file path and stores the mapping.
    /// </summary>
    protected FileRef CreateFileReference(string filePath, bool readOnly)
    {
        Log.Information("Creating file reference for path: {filePath}", filePath);
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
}
