namespace Zylance.Core.Importers.Models;

/// <summary>
///     Result of an import operation.
/// </summary>
public record ParseResult
{
    /// <summary>
    ///     Gets whether the import was successful.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    ///     Gets the error message if the import failed.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    ///     Gets the number of transactions imported.
    /// </summary>
    public int TransactionCount { get; init; }

    /// <summary>
    ///     Gets any warnings generated during import.
    /// </summary>
    public IReadOnlyList<string>? Warnings { get; init; }

    /// <summary>
    ///     The parsed import statements (one per account) produced by the parser.
    /// </summary>
    public required List<ImportStatement> Statements { get; init; }
}
