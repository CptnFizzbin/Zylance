using Zylance.Contract.Models.File;
using Zylance.Core.Importers.Models;

namespace Zylance.Core.Importers.Interfaces;

/// <summary>
///     Interface for importing financial data from various file formats.
/// </summary>
public interface IImporter
{
    /// <summary>
    ///     Gets the supported file extensions for this importer.
    /// </summary>
    IReadOnlyList<(string Name, string[] Extensions)> SupportedExtensions { get; }

    /// <summary>
    ///     Imports transactions from the specified file.
    /// </summary>
    /// <param name="fileRef">Reference to the file to import.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the import operation.</returns>
    Task<ImportResult> ImportAsync(FileRef fileRef, CancellationToken cancellationToken = default);
}
