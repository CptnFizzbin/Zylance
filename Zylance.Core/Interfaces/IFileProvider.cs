using Zylance.Contract.Api.File;

namespace Zylance.Core.Interfaces;

public interface IFileProvider
{
    public bool Exists(string path);

    public FileRef SelectFile(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = true
    );

    public FileRef CreateFile(
        string? title = null,
        string? filename = null,
        (string Name, string[] Extensions)[]? filters = null
    );

    public Stream OpenFile(FileRef fileRef);
    public void SaveFile(FileRef fileRef, Stream content);
    public void DeleteFile(FileRef fileRef);
    public FileRef GetTempFile(string path);
    public FileRef GetAppDataFile(string path);
}

public interface ILocalFileProvider : IFileProvider
{
    public string GetFilePath(FileRef fileRef);
}
