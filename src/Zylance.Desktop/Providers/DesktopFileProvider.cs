using Photino.NET;
using Serilog;
using Zylance.Contract.Models.File;
using Zylance.Core.Logging;
using Zylance.Core.Platform;

namespace Zylance.Desktop.Providers;

/// <summary>
///     Desktop implementation of IFileProvider using Photino's cross-platform file
///     dialogs.
///     Works on Windows, macOS, and Linux using native file dialogs on each
///     platform.
/// </summary>
public class DesktopFileProvider(PhotinoWindow window, string appDataPath, string tempDataPath)
    : LocalFileProvider(appDataPath, tempDataPath)
{
    private static readonly ILogger Log = ZyLogger.ForContext<DesktopFileProvider>();

    /// <summary>
    ///     Shows an open-file dialog and returns a FileRef for the selected file.
    /// </summary>
    /// <param name="title">Optional dialog title.</param>
    /// <param name="filters">File filter options.</param>
    /// <param name="readOnly">Whether the file should be treated as read-only.</param>
    public override async Task<FileRef> SelectFileAsync(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = true
    )
    {
        Log.Information("Prompting user to select a file with title: {Title}", title ?? "Select File");
        var dialogTitle = title ?? (filters is { Length: > 0 } ? $"Select {filters[0].Name}" : "Select File");

        var fileFilters = filters ?? [(Name: "All Files", Extensions: ["*"])];

        var selectedFiles = await window.ShowOpenFileAsync(dialogTitle, null, false, fileFilters);

        if (selectedFiles == null || selectedFiles.Length == 0 || string.IsNullOrEmpty(selectedFiles[0]))
            throw new OperationCanceledException("File selection was cancelled by the user.");

        Log.Information("User selected file: {FilePath}", selectedFiles[0]);
        return CreateFileReference(selectedFiles[0], readOnly);
    }

    /// <summary>
    ///     Shows a save-file dialog and returns a FileRef for the chosen path.
    /// </summary>
    /// <param name="title">Optional dialog title.</param>
    /// <param name="defaultPath">Suggested default path.</param>
    /// <param name="filters">File filter options.</param>
    public override async Task<FileRef> CreateFileAsync(
        string? title = null,
        string? defaultPath = null,
        (string Name, string[] Extensions)[]? filters = null
    )
    {
        Log.Information("Prompting user to select a file with title: {Title}", title ?? "Select File");
        var dialogTitle = title ?? (filters is { Length: > 0 } ? $"Save {filters[0].Name}" : "Save File");

        var fileFilters = filters ?? [(Name: "All Files", Extensions: ["*"])];

        var filePath = await window.ShowSaveFileAsync(dialogTitle, defaultPath, fileFilters);

        if (string.IsNullOrEmpty(filePath))
            throw new OperationCanceledException("File creation was cancelled by the user.");

        Log.Information("User selected file path: {FilePath}", filePath);
        return CreateFileReference(filePath, FileRefFlags.ReadWrite);
    }
}
