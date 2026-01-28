using System.Reflection;
using Photino.NET;
using Zylance.Contract.Api.File;
using Zylance.Core.Lib.Interfaces;

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
    private readonly string _sessionTempDir = Path.Combine(
        Path.GetTempPath(),
        "Zylance",
        Guid.NewGuid().ToString()
    );

    private bool _disposed;

    /// <summary>
    ///     Cleans up the session temp directory and all its contents.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

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

    public FileRef SelectFile(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = true
    )
    {
        var dialogTitle = title
            ?? (filters is { Length: > 0 }
                ? $"Select {filters[0].Name}"
                : "Select File");

        var fileFilters = filters ?? [(Name: "All Files", Extensions: ["*"])];

        var selectedFiles = window.ShowOpenFile(dialogTitle, null, false, fileFilters);

        if (selectedFiles == null || selectedFiles.Length == 0 || string.IsNullOrEmpty(selectedFiles[0]))
            throw new OperationCanceledException("File selection was cancelled by the user.");

        return CreateFileReference(selectedFiles[0], readOnly);
    }

    public FileRef CreateFile(
        string? title = null,
        string? filename = null,
        (string Name, string[] Extensions)[]? filters = null
    )
    {
        var dialogTitle = title
            ?? (filters is { Length: > 0 }
                ? $"Save {filters[0].Name}"
                : "Save File");

        var fileFilters = filters ?? [(Name: "All Files", Extensions: ["*"])];

        var filePath = window.ShowSaveFile(dialogTitle, filename, fileFilters);

        return string.IsNullOrEmpty(filePath)
            ? throw new OperationCanceledException("File creation was cancelled by the user.")
            : CreateFileReference(filePath);
    }

    public Stream OpenFile(FileRef fileRef)
    {
        var filePath = GetFilePath(fileRef);

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}", filePath);

        // Check if the file is read-only on the file system
        var fileInfo = new FileInfo(filePath);
        var fileAccess = fileInfo.IsReadOnly
            ? FileAccess.Read
            : FileAccess.ReadWrite;

        return File.Open(filePath, FileMode.Open, fileAccess, FileShare.Read);
    }

    public void SaveFile(FileRef fileRef, Stream content)
    {
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
        using var fileStream = File.Create(filePath);
        content.CopyTo(fileStream);
    }

    public void DeleteFile(FileRef fileRef)
    {
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
    }

    public FileRef GetTempFile(string path)
    {
        // Combine the path with the session-specific temp directory
        var tempPath = Path.Combine(_sessionTempDir, path);

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(tempPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return CreateFileReference(tempPath);
    }

    public FileRef GetAppDataFile(string path)
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
    ///     Creates a FileRef from a file path and stores the mapping.
    /// </summary>
    private FileRef CreateFileReference(string filePath, bool readOnly = false)
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
}
