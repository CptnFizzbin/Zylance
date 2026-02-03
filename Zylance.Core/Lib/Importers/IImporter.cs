using Zylance.Contract.Models.File;

namespace Zylance.Core.Lib.Importers;

/// <summary>
/// Interface for importing financial data from various file formats.
/// </summary>
public interface IImporter
{
    /// <summary>
    /// Imports transactions from the specified file.
    /// </summary>
    /// <param name="fileRef">Reference to the file to import.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the import operation.</returns>
    Task<ImportResult> ImportAsync(FileRef fileRef, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the supported file extensions for this importer.
    /// </summary>
    IReadOnlyList<string> SupportedExtensions { get; }
}
