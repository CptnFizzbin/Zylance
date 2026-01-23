using Zylance.Contract;

namespace Zylance.Gateway;

/// <summary>
///     Gateway service that wraps IFileProvider and enforces read-only rules through its own registry.
///     Provides an additional security layer by tracking file access permissions independently.
/// </summary>
public class FileService : IFileProvider
{
    private readonly IFileProvider _fileProvider;
    private readonly Lock _lock = new();
    private readonly Dictionary<string, bool> _readOnlyRegistry = new();

    public FileService(IFileProvider fileProvider)
    {
        _fileProvider = fileProvider;
    }

    public bool Exists(string path)
    {
        return _fileProvider.Exists(path);
    }

    public FileRef SelectFile(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = true
    )
    {
        var fileRef = _fileProvider.SelectFile(title, filters, readOnly);

        // Register the file's read-only status in our registry
        RegisterFileRef(fileRef);

        return fileRef;
    }

    public FileRef CreateFile(
        string? title = null,
        string? filename = null,
        (string Name, string[] Extensions)[]? filters = null
    )
    {
        var fileRef = _fileProvider.CreateFile(title, filename, filters);

        // Register newly created files (they're always writable)
        RegisterFileRef(fileRef);

        return fileRef;
    }

    public Stream OpenFile(FileRef fileRef)
    {
        // Enforce our registry's read-only status
        ValidateFileRefExists(fileRef);

        return _fileProvider.OpenFile(fileRef);
    }

    public void SaveFile(FileRef fileRef, Stream content)
    {
        // Strictly enforce read-only through our registry
        ValidateFileRefExists(fileRef);
        EnforceWriteAccess(fileRef);

        _fileProvider.SaveFile(fileRef, content);
    }

    public void DeleteFile(FileRef fileRef)
    {
        // Strictly enforce read-only through our registry
        ValidateFileRefExists(fileRef);
        EnforceWriteAccess(fileRef);

        _fileProvider.DeleteFile(fileRef);

        // Remove from our registry after successful deletion
        lock (_lock)
        {
            _readOnlyRegistry.Remove(fileRef.Id);
        }
    }

    public FileRef GetTempFile(string path)
    {
        var fileRef = _fileProvider.GetTempFile(path);

        // Register temp files (always writable)
        RegisterFileRef(fileRef);

        return fileRef;
    }

    public FileRef GetAppDataFile(string path)
    {
        var fileRef = _fileProvider.GetAppDataFile(path);

        // Register app data files (always writable)
        RegisterFileRef(fileRef);

        return fileRef;
    }

    /// <summary>
    ///     Registers a FileRef in our read-only registry.
    /// </summary>
    private void RegisterFileRef(FileRef fileRef)
    {
        lock (_lock)
        {
            _readOnlyRegistry[fileRef.Id] = fileRef.ReadOnly;
        }
    }

    /// <summary>
    ///     Validates that a FileRef exists in our registry.
    ///     Throws if the FileRef is not registered (potentially tampered with or from another session).
    /// </summary>
    private void ValidateFileRefExists(FileRef fileRef)
    {
        lock (_lock)
        {
            if (!_readOnlyRegistry.ContainsKey(fileRef.Id))
                throw new UnauthorizedAccessException(
                    $"FileRef with ID '{fileRef.Id}' is not registered with this FileService. "
                    + "This may indicate a security violation or use of a FileRef from another session."
                );
        }
    }

    /// <summary>
    ///     Enforces write access by checking our registry's read-only status.
    ///     Throws if either our registry or the FileRef indicates read-only.
    /// </summary>
    private void EnforceWriteAccess(FileRef fileRef)
    {
        bool isReadOnlyInRegistry;

        lock (_lock)
        {
            isReadOnlyInRegistry = _readOnlyRegistry.TryGetValue(fileRef.Id, out var registryValue) && registryValue;
        }

        // Check both our registry AND the FileRef itself
        if (isReadOnlyInRegistry || fileRef.ReadOnly)
            throw new UnauthorizedAccessException(
                $"Cannot modify read-only file: {fileRef.Filename}. "
                + $"Registry status: {isReadOnlyInRegistry}, FileRef status: {fileRef.ReadOnly}"
            );
    }
}
