namespace Zylance.Core.Vault.Models;

/// <summary>
///     Represents a financial account as stored in the vault.
/// </summary>
public class AccountModel
{
    /// <summary>
    ///     Unique identifier for the account.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    ///     Human-readable account name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The account type (e.g., "checking", "savings", "credit").
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    ///     Current ledger balance for the account.
    /// </summary>
    public required decimal Balance { get; init; }

    /// <summary>
    ///     Currency code for the account (e.g., "USD").
    /// </summary>
    public string? Currency { get; init; }

    /// <summary>
    ///     Available balance (may be null if not applicable).
    /// </summary>
    public decimal? AvailableBalance { get; init; }
}
