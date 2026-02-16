using Zylance.Core.Importers.Models;

namespace Zylance.Core.Importers.Interfaces;

/// <summary>
///     Interface for importing financial data from various file formats.
/// </summary>
public interface IImportParser
{
    /// <summary>
    ///     Gets the supported file extensions for this importer.
    /// </summary>
    IReadOnlyList<(string Name, string[] Extensions)> SupportedExtensions { get; }

    /// <summary>
    ///     Imports transactions from the specified file.
    /// </summary>
    /// <param name="file">The file to import</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the import operation.</returns>
    Task<ParseResult> ParseAsync(Stream file, CancellationToken cancellationToken = default);
}
