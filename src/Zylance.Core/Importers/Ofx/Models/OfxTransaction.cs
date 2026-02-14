namespace Zylance.Core.Importers.Ofx.Models;

/// <summary>
/// Represents a single transaction entry from an OFX statement.
/// </summary>
public record OfxTransaction
{
    /// <summary>
    /// Transaction type code (e.g., XFER).
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// Date the transaction was posted.
    /// </summary>
    public required DateTimeOffset DatePosted { get; init; }

    /// <summary>
    /// Amount of the transaction.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// Unique transaction id from the OFX feed.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Payee or transaction name.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Optional transaction memo.
    /// </summary>
    public string? Memo { get; init; }

    /// <summary>
    /// Optional check number for check transactions.
    /// </summary>
    public string? CheckNumber { get; init; }

    /// <summary>
    /// Optional reference number from the source feed.
    /// </summary>
    public string? ReferenceNumber { get; init; }

    /// <summary>
    /// Whether the transaction represents an internal transfer.
    /// </summary>
    public bool IsTransfer { get; init; }
}
