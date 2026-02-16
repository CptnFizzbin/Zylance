using Zylance.Contract.Models.File;
using Zylance.Core.Platform;
using Zylance.Desktop.Providers;

namespace Zylance.Desktop.Tests.TestUtils;

public delegate Task<string> AsyncPathProvider();

public delegate string SyncPathProvider();

/// <summary>
///     A file provider for headless testing that uses callbacks to simulate file
///     selection.
/// </summary>
/// <returns>The absolute path to the file to select</returns>
public delegate Task<string> AsyncSelectFileHandler(
    string? title,
    (string Name, string[] Extensions)[]? filters,
    bool readOnly
);

public delegate string SyncSelectFileHandler(
    string? title,
    (string Name, string[] Extensions)[]? filters,
    bool readOnly
);

/// <summary>
///     A file provider for headless testing that uses callbacks to simulate file
///     creation.
/// </summary>
/// <returns>The absolute path to the file to create</returns>
public delegate Task<string> AsyncCreateFileHandler(
    string? title,
    string? defaultPath,
    (string Name, string[] Extensions)[]? filters
);

public delegate string SyncCreateFileHandler(
    string? title,
    string? defaultPath,
    (string Name, string[] Extensions)[]? filters
);

public class HeadlessFileProvider(string appDataPath, string tempDataPath)
    : LocalFileProvider(appDataPath, tempDataPath)
{
    private AsyncCreateFileHandler _onCreateFile = (_, _, _) =>
        throw new InvalidOperationException(
            "Unexpected call to CreateFile - no callback set. Please set the OnCreateFile callback in your test setup."
        );

    private AsyncSelectFileHandler _onSelectFile = (_, _, _) =>
        throw new InvalidOperationException(
            "Unexpected call to SelectFile - no callback set. Please set the OnSelectFile callback in your test setup."
        );

    public void OnCreateFile(AsyncCreateFileHandler handler)
    {
        _onCreateFile = handler;
    }

    public void OnCreateFile(SyncCreateFileHandler handler)
    {
        OnCreateFile((title, defaultPath, filters) => Task.FromResult(handler(title, defaultPath, filters)));
    }

    public void OnCreateFile(AsyncPathProvider handler)
    {
        OnCreateFile((_, _, _) => handler());
    }

    public void OnCreateFile(SyncPathProvider handler)
    {
        OnCreateFile((_, _, _) => handler());
    }

    public void OnSelectFile(AsyncSelectFileHandler handler)
    {
        _onSelectFile = handler;
    }

    public void OnSelectFile(SyncSelectFileHandler handler)
    {
        OnSelectFile((title, filters, readOnly) => Task.FromResult(handler(title, filters, readOnly)));
    }

    public void OnSelectFile(AsyncPathProvider handler)
    {
        OnSelectFile((_, _, _) => handler());
    }

    public void OnSelectFile(SyncPathProvider handler)
    {
        OnSelectFile((_, _, _) => handler());
    }

    public override async Task<FileRef> SelectFile(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = FileRefFlags.READ_ONLY
    )
    {
        var selectedPath = await _onSelectFile(title, filters, readOnly);
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
        var selectedPath = await _onCreateFile(title, defaultPath, filters);
        if (string.IsNullOrEmpty(selectedPath))
            throw new OperationCanceledException("File selection was cancelled by the user.");

        return await CreateFileReference(selectedPath, FileRefFlags.READ_WRITE);
    }
}
