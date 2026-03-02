using Serilog;
using Zylance.Contract.Models.File;
using Zylance.Core.Logging;
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
    private static readonly ILogger Log = ZyLogger.ForContext<FileService>();
    private readonly Lock _lock = new();
    private readonly Dictionary<string, bool> _readOnlyRegistry = new();

    /// <summary>
    ///     Checks whether a file exists at the given path.
    /// </summary>
    /// <param name="fileRef">File ref to check.</param>
    public bool Exists(FileRef fileRef)
    {
        Log.Information("Checking if file exists: {filename} (ID: {id})", fileRef.Filename, fileRef.Id);
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
        Log.Information(
            "Prompting user to select file with title: {title}, filters: {filters}, readOnly: {readOnly}",
            title,
            filters != null
                ? string.Join(", ", filters.Select(f => $"{f.Name} ({string.Join(", ", f.Extensions)})"))
                : "None",
            readOnly
        );
        var fileRef = await fileProvider.SelectFileAsync(title, filters, readOnly);
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
        Log.Information(
            "Prompting user to create file with title: {title}, filename: {filename}, filters: {filters}",
            title,
            filename,
            filters != null
                ? string.Join(", ", filters.Select(f => $"{f.Name} ({string.Join(", ", f.Extensions)})"))
                : "None"
        );
        var fileRef = await fileProvider.CreateFileAsync(title, filename, filters);
        RegisterFileRef(fileRef);

        return fileRef;
    }

    /// <summary>
    ///     Opens a stream for the specified FileRef.
    /// </summary>
    /// <param name="fileRef">The file reference to open.</param>
    public Stream OpenFile(FileRef fileRef)
    {
        Log.Information("Opening file: {filename} (ID: {id})", fileRef.Filename, fileRef.Id);
        AssertFileRegistered(fileRef);

        return fileProvider.OpenFile(fileRef);
    }

    /// <summary>
    ///     Opens a stream for the specified FileRef and executes the provided action,
    ///     ensuring proper disposal.
    /// </summary>
    /// <param name="fileRef">The file reference to open.</param>
    /// <param name="action">A callback to perform with the file stream</param>
    public async Task<TResult> WithFileAsync<TResult>(FileRef fileRef, Func<Stream, Task<TResult>> action)
    {
        Log.Information("Opening file with action: {filename} (ID: {id})", fileRef.Filename, fileRef.Id);
        AssertFileRegistered(fileRef);

        await using var stream = fileProvider.OpenFile(fileRef);
        return await action(stream);
    }

    /// <summary>
    ///     Saves content to the specified FileRef.
    /// </summary>
    /// <param name="fileRef">The file reference to save to.</param>
    /// <param name="content">Stream content to write.</param>
    public async Task SaveFileAsync(FileRef fileRef, Stream content)
    {
        Log.Information("Saving file: {filename} (ID: {id})", fileRef.Filename, fileRef.Id);
        AssertFileRegistered(fileRef);
        AssertFileWritable(fileRef);

        await fileProvider.SaveFileAsync(fileRef, content);
    }

    /// <summary>
    ///     Deletes the specified FileRef and removes it from the registry.
    /// </summary>
    /// <param name="fileRef">The file reference to delete.</param>
    public async Task DeleteFileAsync(FileRef fileRef)
    {
        Log.Information("Deleting file: {filename} (ID: {id})", fileRef.Filename, fileRef.Id);
        AssertFileRegistered(fileRef);
        AssertFileWritable(fileRef);

        await fileProvider.DeleteFileAsync(fileRef);

        lock (_lock)
        {
            _readOnlyRegistry.Remove(fileRef.Id);
        }
    }

    /// <summary>
    ///     Returns a temporary file reference for the provided path.
    /// </summary>
    /// <param name="path">Relative path for the temporary file.</param>
    public FileRef GetTempFile(string path)
    {
        Log.Information("Getting temporary file: {path}", path);
        var fileRef = fileProvider.GetTempFile(path);
        RegisterFileRef(fileRef);
        return fileRef;
    }

    /// <summary>
    ///     Returns a file reference located in the application's data directory.
    /// </summary>
    /// <param name="path">Relative path within the application data folder.</param>
    public FileRef GetAppDataFile(string path)
    {
        Log.Information("Getting app data file: {path}", path);
        var fileRef = fileProvider.GetAppDataFile(path);
        RegisterFileRef(fileRef);
        return fileRef;
    }

    /// <summary>
    ///     Registers a FileRef in our read-only registry.
    /// </summary>
    private void RegisterFileRef(FileRef fileRef)
    {
        Log.Information(
            "Registering file reference: {filename} (ID: {id}, ReadOnly: {readOnly})",
            fileRef.Filename,
            fileRef.Id,
            fileRef.ReadOnly
        );
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
        Log.Information("Asserting file is registered: {filename} (ID: {id})", fileRef.Filename, fileRef.Id);
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

        Log.Information("Asserting file is writable: {filename} (ID: {id})", fileRef.Filename, fileRef.Id);
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
