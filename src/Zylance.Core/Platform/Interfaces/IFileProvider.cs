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
    public Task<bool> Exists(FileRef fileRef);

    /// <summary>
    ///     Prompts the user to select a file.
    /// </summary>
    /// <param name="title">Optional dialog title.</param>
    /// <param name="filters">Optional file filters.</param>
    /// <param name="readOnly">Whether the selected file should be read-only.</param>
    public Task<FileRef> SelectFile(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = true
    );

    /// <summary>
    ///     Prompts the user to create a new file and returns the resulting FileRef.
    /// </summary>
    /// <param name="title">Optional dialog title.</param>
    /// <param name="defaultPath">Optional default path or file name.</param>
    /// <param name="filters">Optional file filters.</param>
    public Task<FileRef> CreateFile(
        string? title = null,
        string? defaultPath = null,
        (string Name, string[] Extensions)[]? filters = null
    );

    /// <summary>
    ///     Opens a stream for the provided FileRef.
    /// </summary>
    /// <param name="fileRef">The file reference to open.</param>
    public Task<Stream> OpenFile(FileRef fileRef);

    /// <summary>
    ///     Updates the last-access time for the given file reference
    ///     (platform-specific).
    /// </summary>
    /// <param name="fileRef">The file reference to touch.</param>
    public Task TouchFile(FileRef fileRef);

    /// <summary>
    ///     Saves the provided stream content to the given file reference.
    /// </summary>
    /// <param name="fileRef">Target file reference.</param>
    /// <param name="content">Stream content to save.</param>
    public Task SaveFile(FileRef fileRef, Stream content);

    /// <summary>
    ///     Deletes the specified file reference.
    /// </summary>
    /// <param name="fileRef">File reference to delete.</param>
    public Task DeleteFile(FileRef fileRef);

    /// <summary>
    ///     Returns a FileRef representing a temporary file for the specified path.
    /// </summary>
    /// <param name="path">The temporary path to use.</param>
    public Task<FileRef> GetTempFile(string path);

    /// <summary>
    ///     Returns a FileRef located inside the application's data directory.
    /// </summary>
    /// <param name="path">Relative path inside the application data directory.</param>
    public Task<FileRef> GetAppDataFile(string path);
}
