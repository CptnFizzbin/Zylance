using Zylance.Contract.Models.File;
using Zylance.Core.Platform.Interfaces;
using Zylance.Core.System.Services;

namespace Zylance.Desktop.Services;

/// <summary>
///     Provides local file system operations for the desktop application,
///     extending the core FileService with platform-specific file path handling.
/// </summary>
/// <param name="fileProvider">The local file provider implementation.</param>
public class LocalFileService(ILocalFileProvider fileProvider) : FileService(fileProvider)
{
    /// <summary>
    ///     Returns a local filesystem path for the given FileRef.
    /// </summary>
    /// <param name="fileRef">The file reference.</param>
    public string GetFilePath(FileRef fileRef)
    {
        return fileProvider.GetFilePath(fileRef);
    }
}
