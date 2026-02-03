namespace Zylance.Core.Lib.Importers;

/// <summary>
/// Interface for importing financial data from various file formats.
/// </summary>
public interface IImporter
{
    /// <summary>
    /// Determines if the importer can handle the specified file.
    /// </summary>
    /// <param name="filePath">Path to the file to check.</param>
    /// <returns>True if the importer can handle this file format.</returns>
    Task<bool> CanImportAsync(string filePath);

    /// <summary>
    /// Imports transactions from the specified file.
    /// </summary>
    /// <param name="filePath">Path to the file to import.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result of the import operation.</returns>
    Task<ImportResult> ImportAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the supported file extensions for this importer.
    /// </summary>
    IReadOnlyList<string> SupportedExtensions { get; }
}

/// <summary>
/// Result of an import operation.
/// </summary>
public record ImportResult
{
    /// <summary>
    /// Gets whether the import was successful.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Gets the error message if the import failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the number of transactions imported.
    /// </summary>
    public int TransactionCount { get; init; }

    /// <summary>
    /// Gets any warnings generated during import.
    /// </summary>
    public IReadOnlyList<string>? Warnings { get; init; }
}
