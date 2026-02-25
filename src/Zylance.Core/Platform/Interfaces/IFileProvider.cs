using Zylance.Contract.Models.File;

namespace Zylance.Core.Platform.Interfaces;

/// <summary>
///     Provides file selection and manipulation operations used by the
///     application.
/// </summary>
public interface IFileProvider
{
    /// <summary>
    ///     Checks whether the backing file for a fileRef exists.
    /// </summary>
    /// <param name="fileRef">ref to check.</param>
    public bool Exists(FileRef fileRef);

    /// <summary>
    ///     Prompts the user to select a file.
    /// </summary>
    /// <param name="title">Optional dialog title.</param>
    /// <param name="filters">Optional file filters.</param>
    /// <param name="readOnly">Whether the selected file should be read-only.</param>
    /// <param name="token">Cancellation token.</param>
    public Task<FileRef> SelectFileAsync(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = true,
        CancellationToken token = default
    );

    /// <summary>
    ///     Prompts the user to create a new file and returns the resulting FileRef.
    /// </summary>
    /// <param name="title">Optional dialog title.</param>
    /// <param name="defaultPath">Optional default path or file name.</param>
    /// <param name="filters">Optional file filters.</param>
    /// <param name="token">Cancellation token.</param>
    public Task<FileRef> CreateFileAsync(
        string? title = null,
        string? defaultPath = null,
        (string Name, string[] Extensions)[]? filters = null,
        CancellationToken token = default
    );

    /// <summary>
    ///     Opens a stream for the provided FileRef.
    /// </summary>
    /// <param name="fileRef">The file reference to open.</param>
    public Stream OpenFile(FileRef fileRef);

    /// <summary>
    ///     Updates the last-access time for the given file reference
    ///     (platform-specific).
    /// </summary>
    /// <param name="fileRef">The file reference to touch.</param>
    /// <param name="token">Cancellation token.</param>
    public Task TouchFileAsync(FileRef fileRef, CancellationToken token = default);

    /// <summary>
    ///     Saves the provided stream content to the given file reference.
    /// </summary>
    /// <param name="fileRef">Target file reference.</param>
    /// <param name="content">Stream content to save.</param>
    /// <param name="token">Cancellation token.</param>
    public Task SaveFileAsync(FileRef fileRef, Stream content, CancellationToken token = default);

    /// <summary>
    ///     Deletes the specified file reference.
    /// </summary>
    /// <param name="fileRef">File reference to delete.</param>
    /// <param name="token">Cancellation token.</param>
    public Task DeleteFileAsync(FileRef fileRef, CancellationToken token = default);

    /// <summary>
    ///     Returns a FileRef representing a temporary file for the specified path.
    ///     This creates a temporary file if it doesn't already exist, that can
    ///     be used for intermediate storage or processing.
    /// </summary>
    /// <param name="path">The temporary path to use.</param>
    public FileRef GetTempFile(string path);

    /// <summary>
    ///     Returns a FileRef located inside the application's data directory.
    ///     This creates the file if it doesn't already exist, and is used for
    ///     storing application data such as settings or cache files.
    /// </summary>
    /// <param name="path">Relative path inside the application data directory.</param>
    public FileRef GetAppDataFile(string path);
}
