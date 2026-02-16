namespace Zylance.Core.Importers.Models;

/// <summary>
///     Represents the result of an import operation, including counts of imported
///     and skipped entities.
/// </summary>
public record ImportResult
{
    /// <summary>
    ///     Gets the number of accounts successfully imported.
    /// </summary>
    public required int NumAccountsImported { get; init; }

    /// <summary>
    ///     Gets the number of transactions successfully imported.
    /// </summary>
    public required int NumTransactionsImported { get; init; }

    /// <summary>
    ///     Gets the number of transactions that were skipped during import.
    /// </summary>
    public required int NumTransactionsSkipped { get; init; }
}
