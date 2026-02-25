using Zylance.Contract.Models.File;

namespace Zylance.Core.Platform.Interfaces;

/// <summary>
///     File provider with local filesystem operations.
/// </summary>
public interface ILocalFileProvider : IFileProvider
{
    /// <summary>
    ///     Returns a local filesystem path for the given FileRef.
    /// </summary>
    /// <param name="fileRef">The file reference.</param>
    public string GetFilePath(FileRef fileRef);
}
