namespace Zylance.Core.Importers.Ofx.Models;

/// <summary>
/// Represents a balance entry from an OFX statement (ledger or available).
/// </summary>
public record OfxBalance
{
    /// <summary>
    /// The balance amount.
    /// </summary>
    public required decimal Amount { get; init; }

    /// <summary>
    /// The timestamp the balance was reported for.
    /// </summary>
    public required DateTimeOffset AsOfDate { get; init; }

    /// <summary>
    /// Type of the balance (e.g., "LEDGER" or "AVAIL").
    /// </summary>
    public required string Type { get; init; } // "LEDGER" or "AVAIL"
}
