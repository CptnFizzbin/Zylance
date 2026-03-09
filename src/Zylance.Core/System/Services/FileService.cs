using System.Text;
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
public class FileService(IFileProvider fileProvider) : IFileProvider
{
    private static readonly ILogger Log = ZyLogger.ForContext<FileService>();
    private readonly Lock _lock = new();
    private readonly Dictionary<string, bool> _readOnlyRegistry = [];

    /// <inheritdoc />
    public bool Exists(FileRef fileRef)
    {
        Log.Information("Checking if file exists: {filename} (ID: {id})", fileRef.Filename, fileRef.Id);
        return fileProvider.Exists(fileRef);
    }

    /// <inheritdoc />
    public async Task<FileRef> SelectFileAsync(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = true,
        CancellationToken token = default
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
        var fileRef = await fileProvider.SelectFileAsync(title, filters, readOnly, token);
        RegisterFileRef(fileRef);

        return fileRef;
    }

    /// <inheritdoc />
    public async Task<FileRef> CreateFileAsync(
        string? title = null,
        string? filename = null,
        (string Name, string[] Extensions)[]? filters = null,
        CancellationToken token = default
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
        var fileRef = await fileProvider.CreateFileAsync(title, filename, filters, token);
        RegisterFileRef(fileRef);

        return fileRef;
    }

    /// <inheritdoc />
    public Stream OpenFile(FileRef fileRef)
    {
        Log.Information("Opening file: {filename} (ID: {id})", fileRef.Filename, fileRef.Id);
        AssertFileRegistered(fileRef);

        return fileProvider.OpenFile(fileRef);
    }

    /// <inheritdoc />
    public async Task SaveFileAsync(FileRef fileRef, Stream content, CancellationToken token = default)
    {
        Log.Information("Saving file: {filename} (ID: {id})", fileRef.Filename, fileRef.Id);
        AssertFileRegistered(fileRef);
        AssertFileWritable(fileRef);

        await fileProvider.SaveFileAsync(fileRef, content, token);
    }

    /// <inheritdoc />
    public async Task DeleteFileAsync(FileRef fileRef, CancellationToken token = default)
    {
        Log.Information("Deleting file: {filename} (ID: {id})", fileRef.Filename, fileRef.Id);
        AssertFileRegistered(fileRef);
        AssertFileWritable(fileRef);

        await fileProvider.DeleteFileAsync(fileRef, token);

        lock (_lock)
        {
            _readOnlyRegistry.Remove(fileRef.Id);
        }
    }

    /// <inheritdoc />
    public FileRef GetTempFile(string path)
    {
        Log.Information("Getting temporary file: {path}", path);
        var fileRef = fileProvider.GetTempFile(path);
        RegisterFileRef(fileRef);
        return fileRef;
    }

    /// <inheritdoc />
    public FileRef GetAppDataFile(string path)
    {
        Log.Information("Getting app data file: {path}", path);
        var fileRef = fileProvider.GetAppDataFile(path);
        RegisterFileRef(fileRef);
        return fileRef;
    }

    /// <summary>
    ///     Reads the contents of the specified FileRef as a UTF-8 string.
    /// </summary>
    /// <param name="fileRef">The file reference to read from.</param>
    /// <param name="token">Cancellation token.</param>
    public async Task<string> ReadFileAsync(FileRef fileRef, CancellationToken token = default)
    {
        Log.Information("Reading file as text: {filename} (ID: {id})", fileRef.Filename, fileRef.Id);
        AssertFileRegistered(fileRef);

        await using var stream = fileProvider.OpenFile(fileRef);
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return await reader.ReadToEndAsync(token);
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
    ///     Saves string content to the specified FileRef.
    /// </summary>
    /// <param name="fileRef">The file reference to save to.</param>
    /// <param name="content">String content to write.</param>
    /// <param name="token">Cancellation token.</param>
    public async Task SaveFileAsync(FileRef fileRef, string content, CancellationToken token = default)
    {
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        await SaveFileAsync(fileRef, stream, token);
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
