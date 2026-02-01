using Zylance.Contract.Models.File;
using Zylance.Core.Lib;

namespace Zylance.Core.App.Services;

/// <summary>
///     Gateway service that wraps IFileProvider and enforces read-only rules through its own registry.
///     Provides an additional security layer by tracking file access permissions independently.
/// </summary>
public class FileService(IFileProvider fileProvider)
{
    private readonly Lock _lock = new();
    private readonly Dictionary<string, bool> _readOnlyRegistry = new();

    public bool Exists(string path)
    {
        return fileProvider.Exists(path);
    }

    public async Task<FileRef> SelectFile(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = true
    )
    {
        var fileRef = await fileProvider.SelectFile(title, filters, readOnly);
        RegisterFileRef(fileRef);

        return fileRef;
    }

    public async Task<FileRef> CreateFile(
        string? title = null,
        string? filename = null,
        (string Name, string[] Extensions)[]? filters = null
    )
    {
        var fileRef = await fileProvider.CreateFile(title, filename, filters);
        RegisterFileRef(fileRef);

        return fileRef;
    }

    public async Task<Stream> OpenFile(FileRef fileRef)
    {
        AssertFileRegistered(fileRef);

        return await fileProvider.OpenFile(fileRef);
    }

    public async Task SaveFile(FileRef fileRef, Stream content)
    {
        AssertFileRegistered(fileRef);
        AssertFileWritable(fileRef);

        await fileProvider.SaveFile(fileRef, content);
    }

    public async Task DeleteFile(FileRef fileRef)
    {
        AssertFileRegistered(fileRef);
        AssertFileWritable(fileRef);

        await fileProvider.DeleteFile(fileRef);

        lock (_lock)
        {
            _readOnlyRegistry.Remove(fileRef.Id);
        }
    }

    public async Task<FileRef> GetTempFile(string path)
    {
        var fileRef = await fileProvider.GetTempFile(path);
        RegisterFileRef(fileRef);
        return fileRef;
    }

    public async Task<FileRef> GetAppDataFile(string path)
    {
        var fileRef = await fileProvider.GetAppDataFile(path);
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
    private void AssertFileRegistered(FileRef fileRef)
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
    private void AssertFileWritable(FileRef fileRef)
    {
        bool isReadOnlyInRegistry;

        lock (_lock)
        {
            isReadOnlyInRegistry = _readOnlyRegistry.TryGetValue(fileRef.Id, out var registryValue) && registryValue;
        }

        if (isReadOnlyInRegistry || fileRef.ReadOnly)
            throw new UnauthorizedAccessException(
                $"Cannot modify read-only file: {fileRef.Filename}. "
                    + $"Registry status: {isReadOnlyInRegistry}, FileRef status: {fileRef.ReadOnly}"
            );
    }
}
