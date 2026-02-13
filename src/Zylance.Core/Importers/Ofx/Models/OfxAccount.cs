namespace Zylance.Core.Importers.Ofx.Models;

/// <summary>
/// Represents account information parsed from an OFX statement.
/// </summary>
public record OfxAccount
{
    /// <summary>
    /// The account identifier (e.g., account number) from the statement.
    /// </summary>
    public required string AccountId { get; init; }

    /// <summary>
    /// Account type string (e.g., CHECKING, SAVINGS).
    /// </summary>
    public required string AccountType { get; init; }

    /// <summary>
    /// Currency code for the account (ISO 4217).
    /// </summary>
    public required string Currency { get; init; }

    /// <summary>
    /// Optional bank identifier associated with the account.
    /// </summary>
    public string? BankId { get; init; }
}
