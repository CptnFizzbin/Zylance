using Zylance.Contract.Models.File;
using Zylance.Core.Lib.Importers;

namespace Zylance.Core.Importers.Ofx;

/// <summary>
/// Importer for OFX (Open Financial Exchange) files.
/// </summary>
public class OfxImporter : IImporter
{
    /// <summary>
    /// Supported file extensions and display names for this importer.
    /// </summary>
    public IReadOnlyList<(string Name, string[] Extensions)> SupportedExtensions { get; } =
    [("Open Financial Exchange", [".ofx"]), ("Quicken Financial Exchange", [".qfx"])];

    /// <summary>
    /// Imports the provided file and returns an ImportResult.
    /// </summary>
    public Task<ImportResult> ImportAsync(FileRef fileRef, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
