using Photino.NET;
using Zylance.Contract.Models.File;
using Zylance.Desktop.Lib;

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
    public override async Task<FileRef> SelectFile(
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

    public override async Task<FileRef> CreateFile(
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
}
