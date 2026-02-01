using System.Reflection;
using Photino.NET;
using Zylance.Contract.Models.File;
using Zylance.Core.Lib;

namespace Zylance.Desktop;

/// <summary>
///     Desktop implementation of IFileProvider using Photino's cross-platform file dialogs.
///     Works on Windows, macOS, and Linux using native file dialogs on each platform.
/// </summary>
public class DesktopFileProvider(PhotinoWindow window) : ILocalFileProvider, IDisposable
{
    // Store file references in memory - maps FileRef IDs to actual file paths
    private readonly Dictionary<string, string> _fileReferences = new();
    private readonly Lock _lock = new();

    // Create a unique session directory for this instance
    private readonly string _sessionTempDir = Path.Combine(Path.GetTempPath(), "Zylance", Guid.NewGuid().ToString());

    private bool _disposed;

    /// <summary>
    ///     Cleans up the session temp directory and all its contents.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            // Clean up the session temp directory if it exists
            if (Directory.Exists(_sessionTempDir))
                Directory.Delete(_sessionTempDir, true);
        }
        catch (Exception ex)
        {
            // Log but don't throw - cleanup is best-effort
            Console.Error.WriteLine($"Warning: Failed to clean up temp directory {_sessionTempDir}: {ex.Message}");
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    public bool Exists(string path)
    {
        return File.Exists(path);
    }

    public async Task<FileRef> SelectFile(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = true
    )
    {
        var dialogTitle = title ?? (filters is { Length: > 0 } ? $"Select {filters[0].Name}" : "Select File");

        var fileFilters = filters ?? [(Name: "All Files", Extensions: ["*"])];

        var selectedFiles = await window.ShowOpenFileAsync(dialogTitle, null, false, fileFilters);

        if (selectedFiles == null || selectedFiles.Length == 0 || string.IsNullOrEmpty(selectedFiles[0]))
            throw new OperationCanceledException("File selection was cancelled by the user.");

        return await CreateFileReference(selectedFiles[0], readOnly);
    }

    public async Task<FileRef> CreateFile(
        string? title = null,
        string? defaultPath = null,
        (string Name, string[] Extensions)[]? filters = null
    )
    {
        var dialogTitle = title ?? (filters is { Length: > 0 } ? $"Save {filters[0].Name}" : "Save File");

        var fileFilters = filters ?? [(Name: "All Files", Extensions: ["*"])];

        var filePath = await window.ShowSaveFileAsync(dialogTitle, defaultPath, fileFilters);

        return await (
            string.IsNullOrEmpty(filePath)
                ? throw new OperationCanceledException("File creation was cancelled by the user.")
                : CreateFileReference(filePath)
        );
    }

    public async Task<Stream> OpenFile(FileRef fileRef)
    {
        var filePath = await GetFilePath(fileRef);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}", filePath);

        // Check if the file is read-only on the file system
        var fileInfo = new FileInfo(filePath);
        var fileAccess = fileInfo.IsReadOnly ? FileAccess.Read : FileAccess.ReadWrite;

        return File.Open(filePath, FileMode.Open, fileAccess, FileShare.Read);
    }

    public Task TouchFile(FileRef fileRef)
    {
        return SaveFile(fileRef, "");
    }

    public async Task SaveFile(FileRef fileRef, Stream content)
    {
        var filePath = await GetFilePath(fileRef);

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
        using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream);
    }

    public async Task DeleteFile(FileRef fileRef)
    {
        var filePath = await GetFilePath(fileRef);

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
    }

    public Task<FileRef> GetTempFile(string path)
    {
        // Combine the path with the session-specific temp directory
        var tempPath = Path.Combine(_sessionTempDir, path);

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(tempPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return CreateFileReference(tempPath);
    }

    public Task<FileRef> GetAppDataFile(string path)
    {
        // Get the application data directory (roaming)
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // Combine with application-specific directory (using assembly name or a constant)
        var appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Zylance";
        var fullPath = Path.Combine(appDataPath, appName, path);

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return CreateFileReference(fullPath);
    }

    /// <summary>
    ///     Retrieves the actual file path from a FileRef.
    /// </summary>
    public Task<string> GetFilePath(FileRef fileRef)
    {
        lock (_lock)
        {
            if (_fileReferences.TryGetValue(fileRef.Id, out var filePath))
                return Task.FromResult(filePath);
        }

        throw new ArgumentException($"Invalid FileRef ID: {fileRef.Id}", nameof(fileRef));
    }

    private Task SaveFile(FileRef fileRef, string content)
    {
        var writer = new StreamReader(content);
        return SaveFile(fileRef, writer.BaseStream);
    }

    /// <summary>
    ///     Creates a FileRef from a file path and stores the mapping.
    /// </summary>
    private Task<FileRef> CreateFileReference(string filePath, bool readOnly = false)
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

        return Task.FromResult(fileRef);
    }
}
