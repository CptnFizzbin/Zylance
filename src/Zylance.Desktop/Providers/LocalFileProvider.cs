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
public abstract class LocalFileProvider(string appDataPath, string tempDataPath) : ILocalFileProvider, IDisposable
{
    private static readonly ILogger Log = ZyLogger.ForContext<LocalFileProvider>();

    // Store file references in memory - maps FileRef IDs to actual file paths
    private readonly Dictionary<string, string> _fileReferences = new();
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

    /// <inheritdoc />
    public bool Exists(FileRef fileRef)
    {
        var path = GetFilePath(fileRef);
        return File.Exists(path);
    }

    /// <summary>
    ///     Retrieves the actual file path for a FileRef.
    /// </summary>
    public string GetFilePath(FileRef fileRef)
    {
        Log.Information("Retrieving file path for FileRef ID: {FileRefId}", fileRef.Id);
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
        Log.Information("Opening file for FileRef ID: {FileRefId}", fileRef.Id);
        var filePath = GetFilePath(fileRef);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}", filePath);

        Log.Information("Opening file at path: {FilePath}", filePath);
        return File.Open(filePath, FileMode.Open);
    }

    /// <summary>
    ///     Returns a FileRef for a path located in the application's AppData
    ///     directory.
    /// </summary>
    /// <param name="path">Relative path under the app data directory.</param>
    public FileRef GetAppDataFile(string path)
    {
        Log.Information("Getting app data file for file: {path}", path);
        var fullPath = Path.Combine(appDataPath, path);

        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return CreateFileReference(fullPath, FileRefFlags.ReadWrite);
    }

    /// <summary>
    ///     Returns a FileRef for a path located in the session-specific temp
    ///     directory.
    /// </summary>
    /// <param name="path">Relative path under the temp directory.</param>
    public FileRef GetTempFile(string path)
    {
        Log.Information("Getting temporary fileRef for file: {path}", path);
        var tempPath = Path.Combine(tempDataPath, path);

        var directory = Path.GetDirectoryName(tempPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return CreateFileReference(tempPath, FileRefFlags.ReadWrite);
    }

    /// <summary>
    ///     Prompts the user to select an existing file and returns a FileRef
    ///     representing it.
    /// </summary>
    public abstract Task<FileRef> SelectFileAsync(
        // This method is abstract as this requires specific implementation
        // by DesktopFileProvider, or HeadlessFileProvider in tests
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = FileRefFlags.ReadOnly
    );

    /// <summary>
    ///     Prompts the user to create a file and returns a FileRef for the created
    ///     file.
    /// </summary>
    public abstract Task<FileRef> CreateFileAsync(
        // This method is abstract as this requires specific implementation
        // by DesktopFileProvider, or HeadlessFileProvider in tests
        string? title = null,
        string? defaultPath = null,
        (string Name, string[] Extensions)[]? filters = null
    );

    /// <summary>
    ///     Ensures the file exists by saving an empty content string to it.
    /// </summary>
    /// <param name="fileRef">Reference to the file to touch.</param>
    public Task TouchFileAsync(FileRef fileRef)
    {
        Log.Information("Touching file for FileRef ID: {FileRefId}", fileRef.Id);

        if (fileRef.ReadOnly)
            throw new UnauthorizedAccessException($"Cannot touch read-only file reference: {fileRef.Id}");

        var filePath = GetFilePath(fileRef);
        if (File.Exists(filePath))
        {
            var file = File.OpenHandle(filePath, FileMode.Open, FileAccess.Write);
            File.SetLastWriteTime(file, new DateTime());
        }
        else
        {
            SaveFileAsync(fileRef, "");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Saves the provided stream content to the file referenced by the FileRef.
    /// </summary>
    /// <param name="fileRef">Reference to the file to save.</param>
    /// <param name="content">Content stream to write to disk.</param>
    public async Task SaveFileAsync(FileRef fileRef, Stream content)
    {
        if (fileRef.ReadOnly)
            throw new UnauthorizedAccessException($"Cannot save to read-only file reference: {fileRef.Id}");

        Log.Information("Saving content to file for FileRef ID: {FileRefId}", fileRef.Id);
        var filePath = GetFilePath(fileRef);

        // Check if the file exists and is read-only on the file system
        if (File.Exists(filePath))
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.IsReadOnly)
                throw new UnauthorizedAccessException($"Cannot save to read-only file: {filePath}");
        }

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        // Write the stream to the file
        await using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream);
    }

    /// <summary>
    ///     Deletes the file referenced by the given FileRef and removes its internal
    ///     mapping.
    /// </summary>
    /// <param name="fileRef">Reference to the file to delete.</param>
    public Task DeleteFileAsync(FileRef fileRef)
    {
        if (fileRef.ReadOnly)
            throw new UnauthorizedAccessException($"Cannot delete read-only file reference: {fileRef.Id}");

        Log.Information("Deleting file for FileRef ID: {FileRefId}", fileRef.Id);
        var filePath = GetFilePath(fileRef);

        // Check if the file exists and is read-only on the file system
        if (File.Exists(filePath))
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.IsReadOnly)
                throw new UnauthorizedAccessException($"Cannot delete read-only file: {filePath}");

            File.Delete(filePath);
        }

        // Remove the reference from our tracking dictionary
        lock (_lock)
        {
            _fileReferences.Remove(fileRef.Id);
        }

        return Task.CompletedTask;
    }

    private Task SaveFileAsync(FileRef fileRef, string content)
    {
        Log.Information("Saving string content to file for FileRef ID: {FileRefId}", fileRef.Id);
        var writer = new StreamReader(content);
        return SaveFileAsync(fileRef, writer.BaseStream);
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
