using Zylance.Contract.Models.File;
using Zylance.Desktop.Lib;

namespace Zylance.Desktop.Tests.Headless;

/// <summary>
///     A file provider for headless testing that uses callbacks to simulate file
///     selection.
/// </summary>
/// <returns>The absolute path to the file to select</returns>
public delegate Task<string> SelectFileHandler(
    string? title,
    (string Name, string[] Extensions)[]? filters,
    bool readOnly
);

/// <summary>
///     A file provider for headless testing that uses callbacks to simulate file
///     creation.
/// </summary>
/// <returns>The absolute path to the file to create</returns>
public delegate Task<string> CreateFileHandler(
    string? title,
    string? defaultPath,
    (string Name, string[] Extensions)[]? filters
);

public class HeadlessFileProvider(string appDataPath, string tempDataPath)
    : LocalFileProvider(appDataPath, tempDataPath)
{
    public CreateFileHandler OnCreateFile = (_, _, _) =>
        throw new InvalidOperationException(
            "Unexpected call to CreateFile - no callback set. Please set the OnCreateFile callback in your test setup."
        );

    public SelectFileHandler OnSelectFile = (_, _, _) =>
        throw new InvalidOperationException(
            "Unexpected call to SelectFile - no callback set. Please set the OnSelectFile callback in your test setup."
        );

    public override async Task<FileRef> SelectFile(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = true
    )
    {
        var selectedPath = await OnSelectFile(title, filters, readOnly);
        if (string.IsNullOrEmpty(selectedPath))
            throw new OperationCanceledException("File selection was cancelled by the user.");

        return await CreateFileReference(selectedPath, readOnly);
    }

    public override async Task<FileRef> CreateFile(
        string? title = null,
        string? defaultPath = null,
        (string Name, string[] Extensions)[]? filters = null
    )
    {
        var selectedPath = await OnCreateFile(title, defaultPath, filters);
        if (string.IsNullOrEmpty(selectedPath))
            throw new OperationCanceledException("File selection was cancelled by the user.");

        return await CreateFileReference(selectedPath);
    }
}
