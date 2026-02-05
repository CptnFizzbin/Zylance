using Zylance.Contract.Models.File;
using Zylance.Core.Lib;

namespace Zylance.Core.Tests.Mocks;

internal class MockFileProvider : IFileProvider
{
    public bool Exists(string path)
    {
        return true;
    }

    public Task<FileRef> SelectFile(
        string? title = null,
        (string Name, string[] Extensions)[]? filters = null,
        bool readOnly = true
    )
    {
        return Task.FromResult(
            new FileRef
            {
                Id = "mock",
                Filename = "mock.qfx",
                ReadOnly = readOnly,
            }
        );
    }

    public Task<FileRef> CreateFile(
        string? title = null,
        string? defaultPath = null,
        (string Name, string[] Extensions)[]? filters = null
    )
    {
        return Task.FromResult(
            new FileRef
            {
                Id = "mock",
                Filename = "mock.qfx",
                ReadOnly = false,
            }
        );
    }

    public Task<Stream> OpenFile(FileRef fileRef)
    {
        return Task.FromResult<Stream>(new MemoryStream());
    }

    public Task TouchFile(FileRef fileRef)
    {
        return Task.CompletedTask;
    }

    public Task SaveFile(FileRef fileRef, Stream content)
    {
        return Task.CompletedTask;
    }

    public Task DeleteFile(FileRef fileRef)
    {
        return Task.CompletedTask;
    }

    public Task<FileRef> GetTempFile(string path)
    {
        return Task.FromResult(
            new FileRef
            {
                Id = "temp",
                Filename = path,
                ReadOnly = false,
            }
        );
    }

    public Task<FileRef> GetAppDataFile(string path)
    {
        return Task.FromResult(
            new FileRef
            {
                Id = "appdata",
                Filename = path,
                ReadOnly = false,
            }
        );
    }
}
