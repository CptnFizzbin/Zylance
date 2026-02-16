using Zylance.Contract.Models.File;
using Zylance.Core.Platform.Interfaces;

namespace Zylance.Core.System.Services;

/// <summary>
///     Gateway service that wraps IFileProvider and enforces read-only rules
///     through its own registry.
///     Provides an additional security layer by tracking file access permissions
///     independently.
/// </summary>
public class FileService(IFileProvider fileProvider)
{
    private readonly Lock _lock = new();
    private readonly Dictionary<string, bool> _readOnlyRegistry = new();

    /// <summary>
    ///     Checks whether a file exists at the given path.
    /// </summary>
    /// <param name="fileRef">File ref to check.</param>
    public Task<bool> Exists(FileRef fileRef)
    {
        return fileProvider.Exists(fileRef);
    }

    /// <summary>
    ///     Prompts the platform file picker and returns the selected FileRef.
    /// </summary>
    /// <param name="title">Optional dialog title.</param>
    /// <param name="filters">Optional file extension filters.</param>
    /// <param name="readOnly">Whether the selected file should be opened read-only.</param>
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

    /// <summary>
    ///     Prompts the platform create-file dialog and returns the created FileRef.
    /// </summary>
    /// <param name="title">Optional dialog title.</param>
    /// <param name="filename">Optional default file name.</param>
    /// <param name="filters">Optional file extension filters.</param>
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

    /// <summary>
    ///     Opens a stream for the specified FileRef.
    /// </summary>
    /// <param name="fileRef">The file reference to open.</param>
    public async Task<Stream> OpenFileAsync(FileRef fileRef)
    {
        AssertFileRegistered(fileRef);

        return await fileProvider.OpenFile(fileRef);
    }

    /// <summary>
    ///     Opens a stream for the specified FileRef and executes the provided action,
    ///     ensuring proper disposal.
    /// </summary>
    /// <param name="fileRef">The file reference to open.</param>
    /// <param name="action">A callback to perform with the file stream</param>
    public async Task<TResult> WithFileAsync<TResult>(FileRef fileRef, Func<Stream, Task<TResult>> action)
    {
        AssertFileRegistered(fileRef);

        await using var stream = await fileProvider.OpenFile(fileRef);
        return await action(stream);
    }

    /// <summary>
    ///     Saves content to the specified FileRef.
    /// </summary>
    /// <param name="fileRef">The file reference to save to.</param>
    /// <param name="content">Stream content to write.</param>
    public async Task SaveFile(FileRef fileRef, Stream content)
    {
        AssertFileRegistered(fileRef);
        AssertFileWritable(fileRef);

        await fileProvider.SaveFile(fileRef, content);
    }

    /// <summary>
    ///     Deletes the specified FileRef and removes it from the registry.
    /// </summary>
    /// <param name="fileRef">The file reference to delete.</param>
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

    /// <summary>
    ///     Returns a temporary file reference for the provided path.
    /// </summary>
    /// <param name="path">Relative path for the temporary file.</param>
    public async Task<FileRef> GetTempFile(string path)
    {
        var fileRef = await fileProvider.GetTempFile(path);
        RegisterFileRef(fileRef);
        return fileRef;
    }

    /// <summary>
    ///     Returns a file reference located in the application's data directory.
    /// </summary>
    /// <param name="path">Relative path within the application data folder.</param>
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
    ///     Throws if the FileRef is not registered (potentially tampered with or from
    ///     another session).
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
