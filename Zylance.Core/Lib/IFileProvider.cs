using Zylance.Contract.Models.File;

namespace Zylance.Core.Lib;

public interface IFileProvider
{
    public bool Exists(string path);

    public Task<FileRef> SelectFile(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = true
    );

    public Task<FileRef> CreateFile(
        string? title = null,
        string? defaultPath = null,
        (string Name, string[] Extensions)[]? filters = null
    );

    public Task<Stream> OpenFile(FileRef fileRef);
    public Task TouchFile(FileRef fileRef);
    public Task SaveFile(FileRef fileRef, Stream content);
    public Task DeleteFile(FileRef fileRef);
    public Task<FileRef> GetTempFile(string path);
    public Task<FileRef> GetAppDataFile(string path);
}

public interface ILocalFileProvider : IFileProvider
{
    public Task<string> GetFilePath(FileRef fileRef);
}
