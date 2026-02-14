namespace Zylance.Core.Importers.Ofx.Models;

/// <summary>
/// A bank or credit card statement parsed from OFX containing account, balances and transactions.
/// </summary>
public record OfxStatement
{
    /// <summary>
    /// Account information for this statement.
    /// </summary>
    public required OfxAccount Account { get; init; }

    /// <summary>
    /// Ledger balance for the statement.
    /// </summary>
    public required OfxBalance LedgerBalance { get; init; }

    /// <summary>
    /// Optional available balance.
    /// </summary>
    public OfxBalance? AvailableBalance { get; init; }

    /// <summary>
    /// Transactions included in the statement.
    /// </summary>
    public required List<OfxTransaction> Transactions { get; init; }

    /// <summary>
    /// Optional statement start date.
    /// </summary>
    public DateTimeOffset? DateStart { get; init; }

    /// <summary>
    /// Optional statement end date.
    /// </summary>
    public DateTimeOffset? DateEnd { get; init; }
}
