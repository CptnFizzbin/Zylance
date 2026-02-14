using System.ComponentModel.DataAnnotations;

namespace Zylance.Vault.Local.Entities;

/// <summary>
/// Entity Framework entity representing a ledger entry in the local vault.
/// </summary>
public class LedgerEntryEntity
{
    /// <summary>
    /// Unique identifier for the ledger entry.
    /// </summary>
    [Key]
    public required Guid Id { get; init; }

    /// <summary>
    /// Identifier of the account associated with this ledger entry.
    /// </summary>
    public required Guid AccountId { get; set; }

    /// <summary>
    /// Unix timestamp (milliseconds) for when the entry occurred.
    /// </summary>
    public required long Timestamp { get; set; }

    /// <summary>
    /// Payee or counterparty for the ledger entry.
    /// </summary>
    [MaxLength(255)]
    public required string Payee { get; set; }

    /// <summary>
    /// Optional memo or description for the ledger entry.
    /// </summary>
    [MaxLength(255)]
    public required string Memo { get; set; }

    /// <summary>
    /// Monetary amount for the ledger entry (positive or negative).
    /// </summary>
    public required double Amount { get; set; }
}
